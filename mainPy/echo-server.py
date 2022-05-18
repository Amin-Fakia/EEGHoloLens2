import socket
import vedo
import pickle
import json
from queue import Queue
import mne
from vedo import *
import os
import threading
from functions import *
import easygui
from dsi_24_montage import ch_pos, chnls
#from dsi24_montage import ch_pos, chnls
vedo.settings.allowInteraction = 1
edf_file= easygui.fileopenbox(filetypes=["*.edf"])
raw = mne.io.read_raw_edf(edf_file,preload=True).drop_channels(['EEG CM-Pz','EEG X1-Pz','EEG X2-Pz','EEG X3-Pz', 'Trigger'])
dir_path = os.path.dirname(os.path.realpath(__file__))
NUMBER_OF_THREADS = 4
JOB_NUMBER = [1, 2]
v = 0
sp = vedo.Sphere()
plt = vedo.Plotter(axes=0)
data = [0] * 20
ch_names = raw.info["ch_names"]
#headPath= easygui.fileopenbox(filetypes=[".edf"])
headPath = f"{dir_path}/3dmodel/Head.obj"
sensor_locations = get_sensor_3DLocations(ch_pos,["TRG"])

    
mesh = get_mesh(headPath)
vMin = -1
vMax = 1
intrp = RBF_Interpolation(mesh,sensor_locations,data)
mesh.cmap('jet', intrp,vmin=vMin,vmax=vMax)
proj_snsrs = Points(findVert(sensor_locations,mesh),r=12)
print(len(mesh.points()))

queue = Queue()
test_hostname = '127.0.0.1'
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
host,port = socket.gethostbyname(test_hostname ), 8500
print(host)
# s.bind((socket.gethostname(), 8500))
# s.listen(5)
def getRGB(actor, alpha=True, on='points'):
    lut = actor.mapper().GetLookupTable()
    poly = actor.polydata(transformed=False)
    if 'point' in on:
        vscalars = poly.GetPointData().GetScalars()
    else:
        vscalars = poly.GetCellData().GetScalars()
    cols =lut.MapScalars(vscalars, 0,0)
    arr = utils.vtk2numpy(cols)
    if not alpha:
        arr = arr[:, :3]
    return arr
colors = getRGB(mesh).tolist()
def slider1(widget, event):
    value = widget.GetRepresentation().GetValue()
    ch_name = widget.GetRepresentation().GetTitleText()
    ch_idx = mne.pick_channels(ch_names,include=[f"{ch_name}"])
    data[ch_idx[0]] = value
    
    #print(ch_idx[0],value)
    intrp = RBF_Interpolation(mesh,sensor_locations,data)
    mesh.addQuality().cmap('jet', input_array=intrp,arrayName="Quality", on="points",vmin=vMin,vmax=vMax)
    global colors
    colors = getRGB(mesh).tolist()
    
idx = 0
txts = []
for pt in findVert(sensor_locations,mesh):
    txt = Text3D(f"{chnls[idx][4:6]}",pt,s=0.004,c='k')
    txt.followCamera()
    txts.append(txt)
    idx+=1
sensor_pts = Points(sensor_locations,r=9)
ranges = np.linspace(.035,.95,20)
idx = 0
for i in ranges:
    plt.addSlider2D(slider1,0,vMax,pos=[(0.075,i),(.25,i)],titleSize=0.5, title= f"{chnls[idx]}",showValue=False,tubeWidth=0.0026,sliderWidth=0.0070)
    idx+=1


def start():
    print("trying")
    try:
        s.connect((host,port))
        print(f"Connection has been established.")
        
        while True:
            #print("connected")
            # now our endpoint knows about the OTHER endpoint.
            #clientsocket, address = s.accept()
            #d = {"mylist":[v]}
            #msg = pickle.dumps(d)
            #msg = bytes(f"{len(msg):<{HEADERSIZE}}", 'utf-8') + msg
            msg = json.dumps({"mylist":colors})
            time.sleep(1/60)
            
            
            s.sendall(bytes(msg,encoding="utf-8"))
    except Exception as e:
        print(f"Connection coudln't be established. more details: " + str(e))
        

    
    
    s.close()
    
def create_workers():
    for _ in range(NUMBER_OF_THREADS):
        t = threading.Thread(target=work)
        t.daemon = True
        t.start()
def start_turtle():
    
    while True:
        plt.show(mesh,*txts,proj_snsrs,interactive=0)
        time.sleep(1/60)
def work():
    while True:
        x = queue.get()
        if x == 1:
            start()
            
        if x == 2:
            start_turtle()
        queue.task_done()
def create_jobs():
    for x in JOB_NUMBER:
        queue.put(x)

    queue.join()

create_workers()
create_jobs()