"""Drive the real Linux window from captured board pixels; never alter game state."""
from pathlib import Path
from collections import deque
import os, subprocess, time, json, argparse
from PIL import ImageGrab
parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('--output', type=Path, required=True)
root=parser.parse_args().output.resolve();root.mkdir(parents=True, exist_ok=False)
repo=Path(__file__).resolve().parents[1]
assert os.environ.get('DISPLAY'), 'Use an isolated X11/Xvfb DISPLAY'
env=os.environ | {'XDG_CONFIG_HOME':str(root/'config')}
x='xdotool'
assert subprocess.run([x,'search','--name','^Elegant Snake$'],env=env,capture_output=True).returncode != 0
log=(root/'app.log').open('w')
def launch():
 p=subprocess.Popen(['dotnet',str(repo/'bin/Release/net8.0/Snake.dll')],env=env,cwd=repo,stdout=log,stderr=log)
 for _ in range(50):
  q=subprocess.run([x,'search','--name','^Elegant Snake$'],env=env,capture_output=True,text=True)
  if q.returncode==0:
   w=q.stdout.splitlines()[-1];break
  assert p.poll() is None
  time.sleep(.1)
 else: raise RuntimeError('No owned window')
 g=dict(line.split('=') for line in subprocess.check_output([x,'getwindowgeometry','--shell',w],env=env,text=True).splitlines())
 box=(int(g['X']),int(g['Y']),int(g['X'])+int(g['WIDTH']),int(g['Y'])+int(g['HEIGHT']))
 return p,w,box
p,w,box=launch()
def key(k):subprocess.run([x,'key','--window',w,k],env=env,check=True,stdout=subprocess.DEVNULL,stderr=subprocess.PIPE)
def shot():return ImageGrab.grab(bbox=box,xdisplay=env['DISPLAY']).convert('RGB')
def board(im):
 head=None;food=None;occupied=set()
 for y in range(26):
  for xx in range(26):
   for ox,oy in [(-4,0),(0,-4),(0,0),(4,0),(0,4)]:
    r,g,b=im.getpixel((18+xx*24+12+ox,78+y*24+12+oy))
    if g>r+20 and g>b+2:
     occupied.add((xx,y))
     if g>175:head=(xx,y)
    if (r>215 and g<190) or (r>220 and 170<g<240 and b<190) or (b>220 and 130<g<220 and b>g+25):food=(xx,y)
 return head,food,occupied
moves=[(1,0,'Right'),(0,1,'Down'),(-1,0,'Left'),(0,-1,'Up')]
def route(head,food,blocked,heading):
 queue=deque([(head,[]) ]);seen={head}
 while queue:
  pos,path=queue.popleft()
  if pos==food:return path
  for dx,dy,k in moves:
   if not path and (dx,dy)==(-heading[0],-heading[1]):continue
   n=(pos[0]+dx,pos[1]+dy)
   if n in seen or n in blocked or not (0<=n[0]<26 and 0<=n[1]<26):continue
   seen.add(n);queue.append((n,path+[(dx,dy,k)]))
 raise RuntimeError('No safe path to food')
try:
 time.sleep(.5);key('2');key('Return');time.sleep(.15)
 shot().save(root/'initial.png');print('Initial board:',board(shot()),flush=True)
 original=None;previous=None;heading=(1,0);commanded=None;steps=0;eaten=False
 deadline=time.monotonic()+14
 while time.monotonic()<deadline:
  im=shot();head,food,occupied=board(im)
  if head is None or food is None:time.sleep(.015);continue
  if original is None:original=food;im.save(root/'playing.png')
  if food!=original:
   eaten=True;im.save(root/'scored.png');break
  if previous is not None and head!=previous:heading=(head[0]-previous[0],head[1]-previous[1]);steps+=1
  if head!=commanded:
   path=route(head,food,occupied-{head},heading)
   if path:key(path[0][2])
   commanded=head
  previous=head;time.sleep(.015)
 assert eaten,'Did not observe a real food collection'
 # Continue straight into a wall in Normal mode so the UI persists the earned score.
 time.sleep(3)
 shot().save(root/'game-over.png')
 scorefile=root/'config/ElegantSnake/scores.txt'
 values=[int(v) for v in scorefile.read_text().splitlines()]
 assert values and values[0]>0, values
 p.terminate();p.wait(timeout=10)
 p,w,box=launch();time.sleep(.3)
 im=shot();im.save(root/'reopened.png')
 im.crop((220,12,295,60)).resize((450,288)).save(root/'best-crop.png')
 ocr=subprocess.check_output(['tesseract',str(root/'best-crop.png'),'stdout','--psm','6'],stderr=subprocess.DEVNULL,text=True)
 assert str(values[0]) in ocr,repr(ocr)
 result={'earned_score':values[0],'steps_observed':steps,'persisted_after_real_game_over':True,'reopened_best_ocr':ocr.strip(),'platform':'Linux X11; no Windows/audio claim'}
 (root/'result.json').write_text(json.dumps(result,indent=2));print(json.dumps(result))
finally:
 if p.poll() is None:p.terminate();p.wait(timeout=10)
 log.close()
