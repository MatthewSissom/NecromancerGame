import numpy as np
import moderngl
import math
import random as rand

#temp?
from PIL import Image

width = 0
height = 0

ctx = 0
vbo = 0
fbo = 0

renders = 0
frameDimensions = 0
pixelDimensions = 0

def init(image, frameCount, resolution):

    global ctx
    global vbo
    global fbo
    global renders
    global frameDimensions
    global pixelDimensions

    roughMaxSize = 2900
    #roughMaxSize = 1000

    #if render will fit into a square image less than the rough max then do one square, otherwise break into multiple renders
    frameDimensions = min(math.ceil(math.sqrt(frameCount)),math.floor(roughMaxSize/resolution))
    renders = math.ceil(frameCount/(frameDimensions*frameDimensions))
    pixelDimensions = resolution * frameDimensions

    ctx = moderngl.create_standalone_context()

    text = ctx.texture(image.size,4,image.tobytes())
    #sampler = ctx.sampler(texture = text, border_color = (1,1,1,1))
    sampler = ctx.sampler(texture = text, border_color = (0,0,0,1))
    sampler.use(0)

    vertices = np.array([[0,0],[0,frameDimensions],[frameDimensions,frameDimensions],[0,0],[frameDimensions,0],[frameDimensions,frameDimensions]])
    vertices = vertices.astype('f4').tobytes()

    vbo = ctx.buffer(vertices)

    #fbo = ctx.simple_framebuffer((128, 128))    
    fbo = ctx.simple_framebuffer((pixelDimensions, pixelDimensions))
    fbo.use()

def exit():
    sampler.release()

def render(scalePerFrame, radWidth = 0.5, circleResolution = 20, ratio = 0):

    points = getRadPoints(radWidth,circleResolution) 

    images = []

    for imageIndex in range(renders):
        prog = getMultiColorModProg(points,scalePerFrame,imageIndex*(scalePerFrame*frameDimensions*frameDimensions))
        vao = ctx.vertex_array(prog, [(vbo, "2f4", "in_vert"),])
        fbo.clear(1, 1, 1, 1.0)
        vao.render()
        images.append(Image.frombytes('RGB', fbo.size, fbo.read(), 'raw', 'RGB', 0, -1))

    return images

def getRadPoints(innerRad,resolution):
    outerRad = 1

    points = []
    rand.seed(31)

    #perform calculations as if outerRad == 1
    adjustedInner = innerRad / outerRad
    resolution = math.floor(resolution/2)
    for x in range(-resolution,resolution):
        for y in range(-resolution,resolution):
            dist = math.sqrt( math.pow(x/resolution,2) + math.pow(y/resolution,2))
            if dist <= 1 and dist >= adjustedInner:
                points.append(((x+ rand.random()*resolution/6)*outerRad,(y+ rand.random()*resolution/6)*outerRad))  #scale values to propper size
    return points   

def getModProg(points,scalePerFrame,baseScale):

    totalPoints = len(points*2)

    pointsStr = "{"
    for point in points:
        pointsStr += str(point[0]) + ','
        pointsStr += str(point[1]) + ','
    pointsStr = pointsStr.rpartition(',')[0]
    pointsStr += "}"

    return ctx.program(
        vertex_shader='''
            #version 330

            const int sqrtFrames = '''+str(frameDimensions)+''';

            in vec2 in_vert;

            out vec2 frame_cord;

            void main() {
                frame_cord = in_vert;
                gl_Position = vec4((in_vert.x-.5f*sqrtFrames)*2/sqrtFrames,(in_vert.y-.5f*sqrtFrames)*-2/sqrtFrames,0,1);
            }
        ''',
        fragment_shader='''
            #version 330

            uniform sampler2D Texture;
            
            in vec2 frame_cord;

            out vec4 f_color;

            const float baseScale = '''+str(baseScale)+''';
            const float scalePerFrame = '''+str(scalePerFrame)+''';
            const int samples = '''+str(totalPoints)+''';
            const float samplePoints['''+str(totalPoints)+'''] = '''+pointsStr+''';
            const int sqrtFrames = '''+str(frameDimensions)+''';

            void main() {
                vec2 text_cord = vec2(mod(frame_cord.x,1),mod(frame_cord.y,1));
                float scale = (frame_cord.x - text_cord.x + (frame_cord.y - text_cord.y)*sqrtFrames) * scalePerFrame + baseScale;
                float count = 0;
                for(int i = 0; i < samples; i += 2)
                {
                    count += 1 - texture(Texture, text_cord + vec2(samplePoints[i]*scale,samplePoints[i+1]*scale))[0];
                }
                count = count * 20 / samples;
                f_color = vec4(mod(count,1),0,0,1);
            }
        ''',
    )

def getMultiColorModProg(points,scalePerFrame,baseScale):

    totalPoints = len(points*2)

    pointsStr = "{"
    for point in points:
        pointsStr += str(point[0]) + ','
        pointsStr += str(point[1]) + ','
    pointsStr = pointsStr.rpartition(',')[0]
    pointsStr += "}"

    return ctx.program(
        vertex_shader='''
            #version 330

            const int sqrtFrames = '''+str(frameDimensions)+''';

            in vec2 in_vert;

            out vec2 frame_cord;

            void main() {
                frame_cord = in_vert;
                gl_Position = vec4((in_vert.x-.5f*sqrtFrames)*2/sqrtFrames,(in_vert.y-.5f*sqrtFrames)*-2/sqrtFrames,0,1);
            }
        ''',
        fragment_shader='''
            #version 330

            uniform sampler2D Texture;
            
            in vec2 frame_cord;

            out vec4 f_color;

            const float baseScale = '''+str(baseScale)+''';
            const float scalePerFrame = '''+str(scalePerFrame)+''';
            const int samples = '''+str(totalPoints)+''';
            const float samplePoints['''+str(totalPoints)+'''] = '''+pointsStr+''';
            const int sqrtFrames = '''+str(frameDimensions)+''';

            void main() {
                vec2 text_cord = vec2(mod(frame_cord.x,1),mod(frame_cord.y,1));
                vec4 color;
                float scale = (frame_cord.x - text_cord.x + (frame_cord.y - text_cord.y)*sqrtFrames) * scalePerFrame + baseScale;
                float redCount = 0;
                float blueCount = 0;
                for(int i = 0; i < samples; i += 2)
                {
                    color = texture(Texture, text_cord + vec2(samplePoints[i]*scale,samplePoints[i+1]*scale));
                    redCount += color[0];
                    blueCount += color[2];
                }
                float count = mod((( (redCount+blueCount) * (redCount*blueCount > 0? 2 : 1) ) * 20 / samples),1);
                redCount = redCount > 0 ? 1 : 0;
                blueCount = blueCount > 0 ? 1 : 0;
                f_color = vec4(count*redCount,0,count*blueCount,1);
            }
        ''',
    )


    

    
                
        
    
