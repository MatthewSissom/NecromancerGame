from PIL import Image
import numpy as np
import math

while(True):
    print("Enter the name of the image to outline")
    fileName = str(input())
    try:
        image = Image.open(fileName)
        image = image.convert("L")
    except:
        print("Error: \"" + fileName + "\" could not be loaded \n")
        continue;
    width, height = image.size
    print(width)
    print(height)
    image = np.array(image)
    outlineImg = np.copy(image)
    
    def checkBounds(point):
        return point [0] >= 0 and point [0] < height \
               and point [1] >= 0 and point [1] < width
    
    def allPoints(width, height):
        x = 0
        y = 0
        while y < height:
            while x < width:
                yield (y,x)
                x+=1
            x = 0
            y+=1

    def getRadPoints(rad, tolerance):
        points = []
        for x in range(-rad,rad):
            for y in range(-rad,rad):
                dist = math.sqrt(x*x + y*y)
                if dist < rad + tolerance and dist > rad - tolerance:
                    points.append((x,y))
        return points

    def adjcentPoints(point,rad, tolerance):
        offsets = getRadPoints(rad, tolerance)
        for o in offsets:
            p = (o[0] + point[0],o[1] + point[1])
            if checkBounds(p):
                yield p      

    rad = 12
    tol = 1
    maxCount = len(getRadPoints(rad,tol))
    
    for point in allPoints(width,height):
        count = 0
        if image[point] <= 20:
            outlineImg[point] = 255
            continue
        for adj in adjcentPoints(point,rad,tol):
            if image[adj] <= 20:
                count += 1
        if not count == 0 and count < maxCount/2:
            outlineImg[point] = 0
        else:
            outlineImg[point] = 255
                
    im = Image.fromarray(outlineImg,'L')
    im.save(fileName.partition(".")[0] + "Outline.png", "PNG")
    print("done")
    break;

    

    
                
        
    
