import outliner
import experiment
import files
import time
import math

frames = 100
baseImage = files.loadBase("multicolor1")

timer = time.perf_counter()

img = 0

for i in range(300,400,10):
    experiment.init(baseImage, frames,i)

    #for rad in range(1,tries+1):
    #radius = 2 * rad / tries
    img = experiment.render(
        .05/frames,
        .25
    )

    print(str(time.perf_counter() - timer) + " seconds to render")
    timer = time.perf_counter()
    
    files.save(img,frames,experiment.frameDimensions)

    print(str(time.perf_counter() - timer) + " seconds to save")

'''
arr = outliner.outline(baseImage)
'''