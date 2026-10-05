"""Launcher regressions without starting a server, model or game."""
import argparse
import json
import os
from pathlib import Path
import sys
import tempfile
import unittest
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import launcher


class LauncherTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name)

    def test_settings_preserve_cloud_and_reuse_pairing_key(self):
        path = self.root / ".env"
        path.write_text("FARM_PAIRING_KEY=change-this-pairing-code\nADAFRUIT_IO_KEY=local-test-secret\n", encoding="utf-8")
        first = launcher.prepare_settings(self.root)
        second = launcher.prepare_settings(self.root)
        self.assertEqual(first, second)
        self.assertEqual(first["ADAFRUIT_IO_KEY"], "local-test-secret")
        self.assertEqual(len(first["FARM_PAIRING_KEY"]), 48)

    def test_pairing_backs_up_connection_without_touching_save(self):
        connection = self.root / "farm-connection.json"
        connection.write_text('{"url":"http://old-pc:8000","key":"old"}', encoding="utf-8")
        save = self.root / "farm-save.json"
        save.write_bytes(b'unchanged-gameplay')
        launcher.pair_game("new", self.root)
        launcher.pair_game("new", self.root)
        self.assertEqual(json.loads(connection.read_text())["key"], "new")
        self.assertEqual(len(list(self.root.glob("farm-connection.before-assistant-*"))), 1)
        self.assertEqual(save.read_bytes(), b'unchanged-gameplay')

    def run_start(self, *, generated=True, bind="0.0.0.0", lan=False, occupied=True, verify=True):
        (self.root / "MODEL-MANIFEST.json").write_text(json.dumps({"name":"qwen3:1.7b", "manifest_digest":"verified"}))
        args = argparse.Namespace(ollama="ollama.exe", models=None, no_game=False, lan=lan, mute=True, verify_model=verify)
        calls = []

        def response(url, data=None, key="", timeout=5):
            calls.append(url)
            if url.endswith("/api/tags"):
                return {"models":[{"name":"qwen3:1.7b", "digest":"verified"}]}
            if url.endswith("/v1/pair"):
                return {"paired":True}
            if url.endswith("/health"):
                return {"model_ready":True,"service":"nongtrai","listen_host":bind}
            return {"generated":generated,"answer":"20 xu"}

        class Child:
            pid = 43210
            def poll(self): return None
            def wait(self, **kwargs): return 0
            def __enter__(self): return self
            def __exit__(self, *args): pass
            def terminate(self): pass

        patches = [patch.object(launcher,"ROOT",self.root),
                   patch.object(launcher,"request",side_effect=response),
                   patch.object(launcher,"prepare_settings",return_value={"FARM_PAIRING_KEY":"local-test-only"}),
                   patch.object(launcher,"port_busy",return_value=occupied),
                   patch.object(launcher,"find_game",return_value=self.root/"NongTrai.exe"),
                   patch.object(launcher,"wait_ready"),
                   patch.object(launcher,"pair_game"),
                   patch.object(launcher.subprocess,"Popen",return_value=Child()),
                   patch.object(launcher.subprocess,"run")]
        mocks = [p.start() for p in patches]
        try:
            try:
                launcher.start(args)
                error = None
            except RuntimeError as caught:
                error = str(caught)
            return error, calls, mocks[6].call_count, mocks[7].call_args_list, mocks[8].call_args_list
        finally:
            for p in reversed(patches): p.stop()

    def test_game_starts_only_after_real_answer_contract_and_pairs(self):
        error, calls, pairing, processes, kills = self.run_start()
        self.assertIsNone(error)
        self.assertEqual(pairing, 1)
        self.assertTrue(calls[-1].endswith("/v1/chat"))
        self.assertEqual(len(processes), 1)  # Only game; reused services never killed.
        self.assertIn("-farmMute", processes[0].args[0])
        self.assertEqual(kills, [])

    def test_failed_model_does_not_pair_or_open_game(self):
        error, _, pairing, processes, kills = self.run_start(generated=False)
        self.assertIn("Model chua tra loi", error)
        self.assertEqual((pairing, len(processes), len(kills)), (0,0,0))

    def test_lightweight_start_does_not_load_model(self):
        error, calls, pairing, processes, _ = self.run_start(verify=False)
        self.assertIsNone(error)
        self.assertFalse(any(c.endswith("/v1/chat") or c.endswith("/api/generate") for c in calls))
        self.assertEqual((pairing,len(processes)),(1,1))

    def test_lan_rejects_reusing_pc_only_backend(self):
        error, calls, pairing, processes, _ = self.run_start(bind="127.0.0.1", lan=True)
        self.assertIn("chi mo cho PC", error)
        self.assertEqual((pairing,len(processes)), (0,0))
        self.assertFalse(any(c.endswith("/v1/chat") for c in calls))

    def test_owned_services_cleaned_after_failed_model(self):
        error, _, pairing, processes, kills = self.run_start(generated=False, occupied=False)
        self.assertIn("Model chua tra loi", error)
        self.assertEqual(pairing, 0)
        self.assertEqual(len(processes), 2)
        if os.name == "nt":
            self.assertEqual(len(kills), 2)
            self.assertTrue(all(c.args[0][:3] == ["taskkill","/PID","43210"] for c in kills))


if __name__ == "__main__":
    unittest.main()
