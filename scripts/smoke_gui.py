from pathlib import Path
from PIL import ImageGrab
import subprocess,os,time,json
import argparse
parser=argparse.ArgumentParser(description='Linux X11 smoke: run in an isolated Xvfb display.')
parser.add_argument('--output', type=Path, required=True)
args=parser.parse_args()
repo=Path(__file__).resolve().parents[1];out=args.output.resolve();out.mkdir(parents=True,exist_ok=True)
e=os.environ.copy();e['XDG_CONFIG_HOME']=str(out/'config');e['XDG_CACHE_HOME']=str(out/'cache')
assert e.get('DISPLAY'), 'Run with xvfb-run or your own isolated X11 DISPLAY'
x='xdotool'
existing=subprocess.run([x,'search','--name','^Elegant Snake$'],env=e,capture_output=True,text=True,timeout=3)
assert existing.returncode != 0, 'An existing Snake window is already open; use a separate display'
with (out/'app.log').open('w') as log:
 proc=subprocess.Popen(['dotnet',str(repo/'bin/Release/net8.0/Snake.dll')],cwd=repo,env=e,stdout=log,stderr=subprocess.STDOUT)
 try:
  window=None
  for _ in range(60):
   if proc.poll() is not None:raise RuntimeError('Snake exited with '+str(proc.returncode))
   found=subprocess.run([x,'search','--name','^Elegant Snake$'],env=e,capture_output=True,text=True,timeout=3)
   if found.returncode==0:window=found.stdout.splitlines()[-1];break
   time.sleep(.2)
  assert window,'No app window appeared'
  def key(name):subprocess.run([x,'key','--window',window,name],env=e,check=True,capture_output=True,timeout=3)
  def capture(name):
   geometry=subprocess.check_output([x,'getwindowgeometry','--shell',window],env=e,text=True,timeout=3)
   g={k:int(v) for k,v in (line.split('=') for line in geometry.splitlines())}
   ImageGrab.grab(bbox=(g['X'],g['Y'],g['X']+g['WIDTH'],g['Y']+g['HEIGHT']),xdisplay=e['DISPLAY']).save(out/(name+'.png'))
  time.sleep(.5);capture('ready')
  for digit, label in [('1','easy'), ('2','normal'), ('3','hard')]:
   key(digit);time.sleep(.15);capture('menu-'+label)
  from PIL import ImageChops, Image
  assert ImageChops.difference(Image.open(out/'menu-easy.png').convert('RGB'), Image.open(out/'menu-hard.png').convert('RGB')).getbbox(), 'Difficulty keys did not change the view'
  key('2');key('Return');time.sleep(.2);key('Escape');time.sleep(.2);capture('paused');key('Escape');time.sleep(4);capture('game-over');key('r');time.sleep(.2);key('Escape');time.sleep(.2);capture('restarted-paused')
  assert proc.poll() is None
  # Missing fonts produced zero bright pixels in the header despite a live game.
  from PIL import Image
  for state in ['ready','paused','game-over','restarted-paused']:
   with Image.open(out/(state+'.png')) as shot:
    assert sum(1 for rgb in shot.convert('RGB').crop((20,15,145,55)).getdata() if min(rgb)>180)>100, 'Header text did not render: '+state
  (out/'result.json').write_text(json.dumps({'window_created':True,'keys_sent':['1','2','3','2','Return','Escape','Escape','r','Escape'],'alive_after_restart':True,'screenshots':['ready','menu-easy','menu-normal','menu-hard','paused','game-over','restarted-paused'],'limitations':'No audio or Windows runtime validation; visual states require screenshot inspection.'},indent=2)+'\n')
  print('Snake window, keyboard and restart screenshots saved.')
 finally:
  if proc.poll() is None:proc.terminate();proc.wait(timeout=10)
