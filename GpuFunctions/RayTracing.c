typedef struct {
    float x;
    float y;
    float z;
} Vector3;

float Vector3Length(Vector3 vec) {
    return sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z);
}

float Vector3Dot(Vector3 a, Vector3 b) {
    return a.x * b.x + a.y * b.y + a.z * b.z;
}

Vector3 NormalizeVector3(Vector3 vec) {
    float len = Vector3Length(vec);
    Vector3 ret = { vec.x / len, vec.y / len, vec.z / len };
    return ret;
}

Vector3 Vector3Sum(Vector3 a, Vector3 b) {
    Vector3 ret = { a.x + b.x, a.y + b.y, a.z + b.z };
    return ret;
}

Vector3 Vector3Subtract(Vector3 a, Vector3 b) {
    Vector3 ret = { a.x - b.x, a.y - b.y, a.z - b.z };
    return ret;
}

Vector3 Vector3Multiply(Vector3 a, float b) {
    Vector3 ret = { a.x * b, a.y * b, a.z * b };
    return ret;
}

Vector3 Vector3Mult(Vector3 a, Vector3 b) {
    Vector3 ret = { a.x * b.x, a.y * b.y, a.z * b.z };
    return ret;
}

Vector3 Vector3Divide(Vector3 a, float b) {
    Vector3 ret = { a.x / b, a.y / b, a.z / b };
    return ret;
}

Vector3 RotateVector2(Vector3 vector2, float angle)
{
    float c = cos(angle);
    float s = sin(angle);

    Vector3 result = {0,0,0};
    result.x = vector2.x * c - vector2.y * s;
    result.y = vector2.x * s + vector2.y * c;

    return result;
}

Vector3 RotateVector(Vector3 baseVector, Vector3 rotateVector)
{

    Vector3 YZProj = {baseVector.y, baseVector.z, 0};
    YZProj = RotateVector2(YZProj, rotateVector.x);
    baseVector.y = YZProj.x;
    baseVector.z = YZProj.y;

    Vector3 XZProj = {baseVector.x, baseVector.z, 0};
    XZProj = RotateVector2(XZProj, rotateVector.y);
    baseVector.x = XZProj.x;
    baseVector.z = XZProj.y;


    Vector3 XYProj = {baseVector.x, baseVector.y, 0};
    XYProj = RotateVector2(XYProj, rotateVector.z);
    baseVector.x = XYProj.x;
    baseVector.y = XYProj.y;

    return baseVector;
}

Vector3 Vector3Clamp(Vector3 value, Vector3 a, Vector3 b) {
    Vector3 ret;
    if (value.x > b.x){
        ret.x = b.x;
    }
    if (value.y > b.y){
        ret.y = b.y;
    }
    if (value.z > b.z){
        ret.z = b.z;
    }
    
    if (value.x < a.x){
        ret.x = a.x;
    }
    if (value.y < a.y){
        ret.y = a.y;
    }
    if (value.z < a.z){
        ret.z = a.z;
    }
    return ret;
}

Vector3 Reflect(Vector3 vector, Vector3 normal)
{
    float dot = Vector3Dot(vector, normal);
    return Vector3Subtract(vector, Vector3Multiply(normal , 2 * dot));
}

Vector3 Zero() {
    Vector3 ret = { 0,0,0 };
    return ret;
}

Vector3 Vector3Lerp(Vector3 a, Vector3 b, float t) {
    Vector3 ret = {
        a.x + (b.x - a.x) * t,
        a.y + (b.y - a.y) * t,
        a.z + (b.z - a.z) * t
    };
    return ret;
}

bool Vector3Equal(Vector3 a, Vector3 b) {
    return a.x == b.x && a.y == b.y && a.z == b.z;
}

Vector3 Vector3Cross(Vector3 vector1, Vector3 vector2)
{
    Vector3 ret = {
        (vector1.y * vector2.z) - (vector1.z * vector2.y),
        (vector1.z * vector2.x) - (vector1.x * vector2.z),
        (vector1.x * vector2.y) - (vector1.y * vector2.x)
    };
    return ret;
}

typedef struct {
    Vector3 emmitance;
    Vector3 reflectance;
    float roughness;
    float opacity;
} Material;

typedef struct {
	Vector3 A, B, C;
	Vector3 E1, E2;
	Vector3 normal;
	Material material;
} Triangle;

typedef struct {
	float rate;
	float w;
	float h;
	float fov;
	int samples;
	int bounces;
	float backgroundIntensity;
	float inderectLightIntensity;
	float nIn;
	float nOut;
	int trianglesCount;
	Vector3 cameraPos;
	Vector3 cameraRot;
	Vector3 bgColor;
	Vector3 glColor;

} RenderData;

__global Triangle triangles[10000];

__global float PI = 3.14159265;

__global RenderData renderData[1];

int sample, bounce;
float time;

float fract(float val){
    return val - floor(val);
}

float RandomNoise(float x, float y)
{
    int i = get_global_id(0);
    int j = get_global_id(1);
    float val = sin(x * 12.9898 * i * time + y * 78.233 * j / time) * 43758.5453;
    return fract(val);
}

Vector3 Refract(Vector3 I, Vector3 normal, float ratio)
{
    Vector3 R = { 0,0,0 };
    float k = 1.0 - ratio * ratio * (1.0 - Vector3Dot(normal, I) * Vector3Dot(normal, I));

    if (k >= 0.0)
        R = Vector3Subtract(  Vector3Multiply(I, ratio)  , Vector3Multiply(normal, (float)(ratio * Vector3Dot(normal, I) + sqrt(k)))  );

    return R;
}

Vector3 IdealRefract(Vector3 direction, Vector3 normal)
{
    bool fromOutside = Vector3Dot(normal, direction) < 0.0;

    float ratio = fromOutside ? renderData[0].nOut / renderData[0].nIn : renderData[0].nIn / renderData[0].nOut;
    //float ratio = fromOutside ? 0.98 : 1.02;

    Vector3 refraction, reflection;
    refraction = fromOutside ? Refract(direction, normal, ratio) : Vector3Multiply( Refract(Vector3Multiply(direction, -1.0), normal, ratio), -1.0 );
    reflection = Reflect(direction, normal);

    return Vector3Equal(refraction, Zero()) ? reflection : refraction;
}



float* TriangleIntersect(Triangle triangle, Vector3 origin, Vector3 dir)
{
    float ret[2];


    dir = NormalizeVector3(dir);

    float det = -Vector3Dot(dir, triangle.normal);
    float invdet = 1.0f / det;
    Vector3 AO = Vector3Subtract(origin, triangle.A);
    Vector3 DAO = Vector3Cross(AO, dir);
    float u = Vector3Dot(triangle.E2, DAO) * invdet;
    float v = Vector3Dot(triangle.E1, DAO) * invdet * -1;
    float t = Vector3Dot(AO, triangle.normal) * invdet;

    if (det >= 0.000001f && t >= 0.0f && u >= 0.0f && v >= 0.0f && (u + v) <= 1.0f)
    {
        //*fraction = t;
        ret[0] = 1;
        ret[1] = t;
        return ret;
        //return true;
    }
    else
    {
        ret[0] = 0;
        return ret;
        //return false;
    }
}

Vector3 TracePath(Vector3 origin, Vector3 dir)
{
    Vector3 L = { 0,0,0 };
    Vector3 F = { 1,1,1 };

    bool anyHit = false;
    
    for (int i = 0; i < renderData[0].bounces; i++)
    {
        bounce = i;

        float fraction;
        Vector3 normal;
        Material material;

        dir = NormalizeVector3(dir);

        float far = 1000000;
        float minDistance = far;


        for (int s = 0; s < renderData[0].trianglesCount; s++) {
            float FR = 0;
            Vector3 N = triangles[s].normal;

            float* intersect = TriangleIntersect(triangles[s], origin, dir);
            FR = intersect[1];
            if (intersect[0] == 1 && FR < minDistance)
            {
                minDistance = FR;
                normal = NormalizeVector3(N);
                material = triangles[s].material;
            }
        }

        fraction = minDistance;
        bool hit = minDistance != far;

        if (hit)
        {
            anyHit = true;
            Vector3 newOrigin = Vector3Sum(origin, Vector3Multiply(dir, fraction));

            
            Vector3 randVec = {
                    RandomNoise(bounce + 555, sample + 1111) * 2.0 - 1.0,
                    RandomNoise(bounce + 666, sample + 2222) * 2.0 - 1.0,
                    RandomNoise(bounce + 777, sample + 3333) * 2.0 - 1.0,
            };
            

            randVec = Vector3Multiply(randVec, 1.35);
            Vector3 newDir = RotateVector(normal, randVec);
            
            bool refracted = material.opacity > RandomNoise(bounce + 888, sample + 4444);
            if (refracted)
            {
                Vector3 idealRefraction = IdealRefract(dir, normal);
                newDir = NormalizeVector3(Vector3Lerp(Vector3Multiply(newDir, -1.0), idealRefraction, material.roughness));
                newOrigin = Vector3Sum(newOrigin,  Vector3Multiply( normal, (Vector3Dot(newDir, normal) < 0.0 ? -0.001f : 0.001f)));
            }
            else
            {
                Vector3 idealReflection = Reflect(dir, normal);
                newDir = NormalizeVector3(Vector3Lerp(newDir, idealReflection, material.roughness));
                newOrigin = Vector3Sum(newOrigin, Vector3Multiply( normal, 0.001));
            }
            
            dir = newDir;
            origin = newOrigin;

            Vector3 additionalEmmitance = { 0,0,0 };

            L = Vector3Sum(L, Vector3Mult(F, Vector3Sum(material.emmitance, additionalEmmitance)));
            F = Vector3Mult(F, material.reflectance);
        
        }
        else
        {
            L = Vector3Sum(L, Vector3Multiply(renderData[0].bgColor, renderData[0].inderectLightIntensity));
            F = Zero();
        }
    }
    
    if (anyHit)
    {
        L = Vector3Sum(L, Vector3Multiply(renderData[0].glColor, 0.05));
    }
    else
    {
        L = Vector3Multiply(renderData[0].bgColor, renderData[0].backgroundIntensity);
    }
    
    return Vector3Multiply(L, 255.0);
}


Vector3 Pixel(int i, int j)
{
    Vector3 totalColor = { 0,0,0 };

    float x = (((float)i / (float)renderData[0].w) * 2.0) - 1.0;
    float y = (((float)j / (float)renderData[0].h) * 2.0) - 1.0;

    x *= renderData[0].rate * renderData[0].fov * 0.75;
    y *= -1 * renderData[0].fov * 0.75;

    Vector3 rayDirection = { 1, x, y };

    rayDirection = RotateVector(rayDirection, Vector3Divide(renderData[0].cameraRot, (180 / PI)));

    rayDirection = NormalizeVector3(rayDirection);

    for (int b = 0; b < renderData[0].samples; b++)
    {
        sample = b;
        Vector3 sampleColor = TracePath(renderData[0].cameraPos, rayDirection);
        totalColor = Vector3Sum(totalColor, sampleColor);
    }

    totalColor = Vector3Divide(totalColor, (float)renderData[0].samples);

    Vector3 ma = { 0, 0, 0 };
    Vector3 mb = { 255, 255, 255 };
    //totalColor = Vector3Clamp(totalColor, ma, mb);

    return totalColor;
}


kernel void Render(__global float * RD, __global float * screen, __global float * Time)
{
    time = Time[0];
    //Parse RenderData
    renderData[0].rate = RD[0];
    renderData[0].w = RD[1];
    renderData[0].h = RD[2];
    renderData[0].fov = RD[3];
    renderData[0].backgroundIntensity = RD[4];
    renderData[0].inderectLightIntensity = RD[5];
    renderData[0].nIn = RD[6];
    renderData[0].nOut = RD[7];
    renderData[0].samples = RD[8];
    renderData[0].bounces = RD[9];
    renderData[0].trianglesCount = RD[10];

    Vector3 cameraPos = { RD[11], RD[12], RD[13] };
    renderData[0].cameraPos = cameraPos;

    Vector3 cameraRot = { RD[14], RD[15], RD[16] };
    renderData[0].cameraRot = cameraRot;

    Vector3 bgColor = { RD[17], RD[18], RD[19] };
    renderData[0].bgColor = bgColor;

    Vector3 glColor = { RD[20], RD[21], RD[22] };
    renderData[0].glColor = glColor;

    
    for(int t = 0; t < renderData[0].trianglesCount; t++ )
    {
        int st = 26 * t;
        Vector3 A = {RD[st + 23], RD[st + 24], RD[st + 25]};
        triangles[t].A = A;
        Vector3 B = {RD[st + 26], RD[st + 27], RD[st + 28]};
        triangles[t].B = B;
        Vector3 C = {RD[st + 29], RD[st + 30], RD[st + 31]};
        triangles[t].C = C;
        Vector3 E1 = {RD[st + 32], RD[st + 33], RD[st + 34]};
        triangles[t].E1 = E1;
        Vector3 E2 = {RD[st + 35], RD[st + 36], RD[st + 37]};
        triangles[t].E2 = E2;
        Vector3 normal = {RD[st + 38], RD[st + 39], RD[st + 40]};
        triangles[t].normal = normal;
        Vector3 emmitance = {RD[st + 41], RD[st + 42], RD[st + 43]};
        triangles[t].material.emmitance = emmitance;
        Vector3 reflectance = {RD[st + 44], RD[st + 45], RD[st + 46]};
        triangles[t].material.reflectance = reflectance;
        triangles[t].material.roughness = RD[st + 47];
        triangles[t].material.opacity = RD[st + 48];
        
    }
    


    //render
    int i = get_global_id(0);
    int j = get_global_id(1);
    Vector3 color = Pixel(i, j);
    int location = (j * renderData[0].w + i) * 3;
    screen[location] = color.x;
    screen[location + 1] = color.y;
    screen[location + 2] = color.z;
}