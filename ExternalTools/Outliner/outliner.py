import numpy as np
import moderngl
import math

#temp?
from PIL import Image

width = 0
height = 0

def outline(image, rad = 0.1, radWidth = 0.05, circleResolution = 20, ratio = 0):
    ctx = moderngl.create_standalone_context()

    yScale = image.size[0] / image.size[1]
    points = getRadPoints(rad - radWidth,rad,circleResolution,yScale)
    totalPoints = len(points*2)

    pointsStr = "{"
    for point in points:
        pointsStr += str(point[0]) + ','
        pointsStr += str(point[1]) + ','
    pointsStr = pointsStr.rpartition(',')[0]
    pointsStr += "}"

    prog = ctx.program(
        vertex_shader='''
            #version 330

            in vec2 in_vert;

            out vec2 text_cord;

            void main() {
                text_cord = in_vert;
                gl_Position = vec4((in_vert.x-.5f)*2,(in_vert.y-.5f)*-2,0,1);
            }
        ''',
        fragment_shader='''
            #version 330

            in vec2 text_cord;
            out vec4 f_color;
            uniform sampler2D Texture;

            const int samples = '''+str(totalPoints)+''';
            const float ratio = '''+str(ratio)+''';
            const float samplePoints['''+str(totalPoints)+'''] = '''+pointsStr+''';

            void main() {
                float count = 0;
                for(int i = 0; i < samples; i += 2)
                {
                    count += 1 - texture(Texture, text_cord + vec2(samplePoints[i],samplePoints[i+1]))[0];
                }
                if(count/float(samples) > ratio)
                    f_color = vec4(0,0,0,1);
                else
                    f_color = vec4(1,1,1,1);
                f_color += vec4(1,1,1,1) - texture(Texture, text_cord);
            }
        ''',
    )

    text = ctx.texture(image.size,4,image.tobytes())
    sampler = ctx.sampler(texture = text, border_color = (1,1,1,1))
    sampler.use(0)

    #prog['Texture'] = 0 

    vertices = np.array([[0,0],[0,1],[1,1],[0,0],[1,0],[1,1]])
    vertices = vertices.astype('f4').tobytes()

    vbo = ctx.buffer(vertices)
    vao = ctx.vertex_array(prog, [(vbo, "2f4", "in_vert"),])

    fbo = ctx.simple_framebuffer(image.size)
    fbo.use()
    fbo.clear(1, 1, 1, 1.0)
    vao.render()

    sampler.release()

    return Image.frombytes('RGB', fbo.size, fbo.read(), 'raw', 'RGB', 0, -1)

def getRadPoints(innerRad, outerRad, resolution,yScale):
    points = []

    #perform calculations as if outerRad == 1
    adjustedInner = innerRad / outerRad
    resolution = math.floor(resolution/2)
    for x in range(-resolution,resolution):
        for y in range(-resolution,resolution):
            dist = math.sqrt( math.pow(x/resolution,2) + math.pow(y/resolution,2))
            if dist <= 1 and dist >= adjustedInner:
                points.append((x*outerRad,y*outerRad*yScale))  #scale values to propper size
    return points   


    

    
                
        
    
