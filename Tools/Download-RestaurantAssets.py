"""Download selected CC0 source assets from their authors. No account or paid assets."""
from pathlib import Path
import concurrent.futures, html, io, json, re, requests, zipfile
ROOT=Path(__file__).resolve().parents[1]/'Assets/ThirdParty/Restaurant'
ROOT.mkdir(parents=True,exist_ok=True)
def get(url):
    r=requests.get(url,timeout=90);r.raise_for_status();return r
def entries(folder):
    s=get('https://drive.google.com/drive/folders/'+folder).text
    result=[]
    for ident,row in re.findall(r'<tr data-selectable data-id="([^"]+)"(.*?)</tr>',s,re.S):
        m=re.search(r'data-tooltip="([^"]+)"',row)
        if m: result.append((ident,html.unescape(m[1])))
    return result
jobs=[]
def drive_file(folder,ident,name):
    dest=ROOT/folder/name
    if dest.exists() and dest.stat().st_size>100:return
    data=get('https://drive.google.com/uc?export=download&id='+ident).content
    if data.lstrip().startswith(b'<!DOCTYPE') or data.lstrip().startswith(b'<html'):raise ValueError('HTML instead of '+name)
    dest.parent.mkdir(parents=True,exist_ok=True);dest.write_bytes(data);print(folder+'/'+name, len(data),flush=True)
for folder,drive in [('Sushi','1ATw9R3cCMu6Fi79HALKiRR8Khd9fWmbf'),('Sushi','1NbS5mAHcYMtiiyDjgLp4OBrXTszVxnW6')]:
    for ident,label in entries(drive):
        name=label.split(' Binary')[0]
        if name.endswith('.fbx') and not any(x in name for x in ['Truck','Torii','Shoji','Stains']):jobs.append((folder,ident,name))
for ident,label in entries('1srOaThdSqYBtmrqq9couZShM5r0cw_bH'):
    if label.startswith('License'):jobs.append(('Sushi',ident,'License.txt'))
for ident,label in entries('1SNK9PwPi8xqqxmpU5xEZeiQjB26C1oX6'):
    if label.startswith('License'):jobs.append(('Interior',ident,'License.txt'))
    if label.lower().startswith('fbx'):
        for fid,flabel in entries(ident):
            name=flabel.split(' Binary')[0]
            if name.endswith('.fbx') and any(x in name.lower() for x in ['toilet','sink','trash','lamp','bathroom','door']):jobs.append(('Interior',fid,name))
with concurrent.futures.ThreadPoolExecutor(max_workers=6) as pool:
    for f in [pool.submit(drive_file,*j) for j in jobs]:
        try:f.result()
        except Exception as e:print('FAILED',str(e),flush=True)
for pack in ['mini-characters','building-kit','furniture-kit','food-kit']:
    destination=ROOT/pack
    if destination.is_dir() and any(destination.glob('*.fbx')) and any(destination.glob('*.txt')):
        print('Already present: Kenney '+pack,flush=True);continue
    page=get('https://kenney.nl/assets/'+pack).text
    links=re.findall(r'https://kenney.nl/[^\s\"\x27<>]+\.zip',page)
    if not links:raise ValueError('No official ZIP '+pack)
    z=zipfile.ZipFile(io.BytesIO(get(links[-1]).content))
    for name in z.namelist():
        if name.endswith('/') or not (name.lower().endswith(('.fbx','.png','.txt'))):continue
        if name.lower().endswith('.png') and 'colormap' not in name.lower():continue
        if 'character-' in name and not any('character-'+s in name for s in ['male-a','male-b','male-c','female-a','female-b','female-c']):continue
        dest=destination/Path(name).name;dest.parent.mkdir(parents=True,exist_ok=True);dest.write_bytes(z.read(name))
    print('Kenney '+pack+' OK',flush=True)
for asset in ['dining_table','dining_chair_02','standing_chalkboard_01','brass_pan_01','food_apple_01','strawberry_chocolate_cake']:
    files=get('https://api.polyhaven.com/files/'+asset).json();fmt=files['fbx']['1k']['fbx'];folder=ROOT/asset;folder.mkdir(exist_ok=True)
    (folder/(asset+'.fbx')).write_bytes(get(fmt['url']).content)
    for name,info in fmt.get('include',{}).items():
        dest=folder/name;dest.parent.mkdir(parents=True,exist_ok=True);dest.write_bytes(get(info['url']).content)
    (folder/'License.txt').write_text('CC0 1.0 Universal\nSource: https://polyhaven.com/a/'+asset+'\nLicense: https://polyhaven.com/license\n',encoding='utf-8')
    print('Poly Haven '+asset+' OK',flush=True)
for asset in ['WoodFloor023','Tiles074']:
    data=get('https://ambientcg.com/get?file='+asset+'_1K-JPG.zip').content
    z=zipfile.ZipFile(io.BytesIO(data));folder=ROOT/asset;folder.mkdir(exist_ok=True)
    for name in z.namelist():
        if name.endswith(('.jpg','.txt')):(folder/Path(name).name).write_bytes(z.read(name))
    (folder/'License.txt').write_text('CC0 1.0 Universal\nhttps://ambientcg.com/view?id='+asset+'\nhttps://docs.ambientcg.com/license/\n',encoding='utf-8')
    print('ambientCG '+asset+' OK',flush=True)
