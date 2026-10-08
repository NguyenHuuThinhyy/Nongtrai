// Copyright TriForge. Local GGUF inference; no socket, service or subprocess.
#include "llama.h"
#include <atomic>
#include <algorithm>
#include <cstring>
#include <mutex>
#include <string>
#include <vector>

#if defined(_WIN32)
#define FARM_API extern "C" __declspec(dllexport)
#else
#define FARM_API extern "C" __attribute__((visibility("default")))
#endif

struct Engine {
    llama_model *model = nullptr;
    llama_context *context = nullptr;
    std::atomic<bool> cancelled{false};
    std::mutex text_mutex;
    std::string output, error;
    ~Engine() { if (context) llama_free(context); if (model) llama_model_free(model); }
};
static bool abort_decode(void *p) { return static_cast<Engine *>(p)->cancelled.load(); }
static void quiet_log(ggml_log_level, const char *, void *) {}
static int fail(Engine *e, const char *message) {
    std::lock_guard<std::mutex> guard(e->text_mutex);
    e->error = message;
    return -1;
}
FARM_API void * farm_chat_create() {
    static std::once_flag once;
    std::call_once(once, [] { llama_log_set(quiet_log, nullptr); llama_backend_init(); });
    return new Engine();
}
FARM_API int farm_chat_load(void *handle, const char *path, int threads) {
    auto *e = static_cast<Engine *>(handle);
    try {
        auto mp = llama_model_default_params();
        mp.n_gpu_layers = 0; mp.use_mmap = true;
        mp.progress_callback = [](float, void *data) { return !static_cast<Engine *>(data)->cancelled.load(); };
        mp.progress_callback_user_data = e;
        e->model = llama_model_load_from_file(path, mp);
        if (!e->model) return fail(e, "Khong nap duoc model GGUF. Kiem tra file va bo nho.");
        auto cp = llama_context_default_params();
        cp.n_ctx = 2048; cp.n_batch = 128; cp.n_ubatch = 64;
        cp.n_threads = cp.n_threads_batch = std::max(1, std::min(threads, 4));
        cp.abort_callback = abort_decode; cp.abort_callback_data = e;
        e->context = llama_init_from_model(e->model, cp);
        if (!e->context) return fail(e, "Khong du bo nho tao ngu canh AI.");
        return 0;
    } catch (const std::exception &ex) { return fail(e, ex.what()); }
}
FARM_API void farm_chat_cancel(void *handle) {
    if (handle) static_cast<Engine *>(handle)->cancelled.store(true);
}
FARM_API void farm_chat_reset(void *handle) {
    if (handle) {
        auto *e = static_cast<Engine *>(handle);
        e->cancelled.store(false);
        std::lock_guard<std::mutex> guard(e->text_mutex);
        e->output.clear(); e->error.clear();
    }
}
// Only the worker thread mutates model/context. Cancellation and reading text
// are the only functions called concurrently; destruction waits for the worker.
FARM_API int farm_chat_generate(void *handle, const char *prompt, int limit) {
    auto *e = static_cast<Engine *>(handle);
    if (!e || !e->context) return -1;
    { std::lock_guard<std::mutex> guard(e->text_mutex); e->output.clear(); e->error.clear(); }
    llama_sampler *sampler = nullptr;
    try {
        llama_memory_clear(llama_get_memory(e->context), true);
        const auto *vocab = llama_model_get_vocab(e->model);
        const int length = static_cast<int>(std::strlen(prompt));
        int count = llama_tokenize(vocab, prompt, length, nullptr, 0, false, true);
        if (count >= 0) return fail(e, "Khong tach duoc token.");
        std::vector<llama_token> tokens(-count);
        count = llama_tokenize(vocab, prompt, length, tokens.data(), tokens.size(), false, true);
        if (count <= 0 || count > 1650) return fail(e, "Cau hoi/ngu canh qua dai. Hay hoi ngan hon.");
        tokens.resize(count);
        for (int pos = 0; pos < count; pos += 128) {
            if (e->cancelled.load()) return 1;
            auto batch = llama_batch_get_one(tokens.data() + pos, std::min(128, count-pos));
            if (llama_decode(e->context, batch)) return e->cancelled.load() ? 1 : fail(e, "AI khong xu ly duoc cau hoi.");
        }
        sampler = llama_sampler_chain_init(llama_sampler_chain_default_params());
        llama_sampler_chain_add(sampler, llama_sampler_init_top_k(40));
        llama_sampler_chain_add(sampler, llama_sampler_init_top_p(.9f, 1));
        llama_sampler_chain_add(sampler, llama_sampler_init_temp(.6f));
        llama_sampler_chain_add(sampler, llama_sampler_init_dist(12345));
        int result = 0;
        for (int i = 0; i < std::min(limit, 384); ++i) {
            if (e->cancelled.load()) { result = 1; break; }
            llama_token token = llama_sampler_sample(sampler, e->context, -1);
            if (llama_vocab_is_eog(vocab, token)) break;
            char small[256];
            int size = llama_token_to_piece(vocab, token, small, sizeof(small), 0, false);
            std::string piece;
            if (size < 0) {
                std::vector<char> large(-size);
                size = llama_token_to_piece(vocab, token, large.data(), large.size(), 0, false);
                if (size > 0) piece.assign(large.data(), size);
            } else if (size > 0) piece.assign(small, size);
            { std::lock_guard<std::mutex> guard(e->text_mutex); e->output += piece; }
            if (i + 1 == std::min(limit, 384)) break;
            auto batch = llama_batch_get_one(&token, 1);
            if (llama_decode(e->context, batch)) { result = e->cancelled.load() ? 1 : fail(e, "AI dung khi dang tra loi."); break; }
        }
        llama_sampler_free(sampler);
        return result;
    } catch (const std::exception &ex) {
        if (sampler) llama_sampler_free(sampler);
        return fail(e, ex.what());
    }
}
FARM_API int farm_chat_read(void *handle, char *buffer, int capacity, int error) {
    auto *e = static_cast<Engine *>(handle);
    if (!e || !buffer || capacity <= 0) return 0;
    std::lock_guard<std::mutex> guard(e->text_mutex);
    const auto &text = error ? e->error : e->output;
    const int count = std::min(static_cast<int>(text.size()), capacity - 1);
    std::memcpy(buffer, text.data(), count); buffer[count] = 0;
    return count;
}
FARM_API void farm_chat_destroy(void *handle) { delete static_cast<Engine *>(handle); }
