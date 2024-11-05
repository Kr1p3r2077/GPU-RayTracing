using Cloo;
using OpenCLTemplate;
using RayTracing;


//X - forward
//Y - right
//Z - up
CLCalc.InitCL();
List<ComputeDevice> L = CLCalc.CLDevices;
CLCalc.Program.DefaultCQ = 0;

SceneReader.ReadSceneFromFile("kornelBox.txt");
ScreenGPU.RenderGPU("test.png", true);

//SceneReader.ReadSceneFromFile("sputnik/SputnikScene.txt");
//SceneReader.ReadSceneFromFile("scene1.txt");
//SceneReader.ReadSceneFromFile("sputnik/SputnikScene.txt");
//Screen.RenderThreaded(1920/4, 1080/4, $"scene.png");
//Screen.RenderThreaded(1920/4,1080/4,"test1.png");
