__IN_PATH__ = "BaseImages/";
__OUT_PATH__ = "Outlines/";
__baseName__ = ""

from PIL import Image
from pathlib import Path
import math

def loadBase(debug = False):
    while True :
        if type(debug) is bool:
            __baseName__ = "Untitled"
        elif type(debug) is str:
            __baseName__ = debug
        else:
            print("Enter the name of the image to outline")
            __baseName__ = str(input()).partition('.')[0]
        inPath = Path(__IN_PATH__ + __baseName__ + ".png")
        if not inPath.exists():
            print("Error: \"" + str(inPath) + "\" could not be found \n make sure the image is in BaseImages \n")
            continue
        number = 2

        try:
            image = Image.open(inPath)
            image = image.convert("RGBA")
        except:
            print("Error: \"" + str(fileName) + "\" could not be loaded \n")
            continue
        print("Image loaded")
        return image

def saveOutline(img):
    outPath = Path(__OUT_PATH__ + __baseName__ + "Outline.png")
    number = 2
    while outPath.exists():
        outPath = Path(__OUT_PATH__ + __baseName__ + "Outline" + str(number) + ".png")
        number += 1
    img.save(outPath, "PNG")

def saveGIF(imgList):
    outPath = Path(__OUT_PATH__ + __baseName__ + "Animated.gif")
    number = 2
    while outPath.exists():
        outPath = Path(__OUT_PATH__ + __baseName__ + "Animated" + str(number) + ".gif")
        number += 1

    imgList[0].save(outPath, save_all = True, append_images = imgList[1:], loop = 0, duration = 40);

def saveSpriteSheet(img, frameDimension):
    saveOutline(img)
    imList = spriteSheetToList(img,frameDimension)
    saveGIF(imList)
            

def spriteSheetToList(img, frameDimension):
    imList = []
    frameSize = img.size[0]/frameDimension;
    for y in range(frameDimension):
        for x in range(frameDimension):
            tup = (math.floor(x*frameSize),math.floor(y*frameSize),math.floor((x+1)*frameSize),math.floor((y+1)*frameSize))
            imList.append(img.crop(tup))
    return imList

def save(renderOutput,frameCount,frameDimension):
    imgList = []
    if type(renderOutput) is list:
        for sheet in renderOutput:
            imgList += spriteSheetToList(sheet, frameDimension)
    else:
        imgList += spriteSheetToList(renderOutput, frameDimension)
    imgList = imgList[:frameCount]

    saveGIF(imgList)

