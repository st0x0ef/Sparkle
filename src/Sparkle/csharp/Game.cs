using System.Reflection;
using Raylib_cs;
using Sparkle.csharp.content;
using Sparkle.csharp.graphics;
using Sparkle.csharp.scene;
using Sparkle.csharp.window;

namespace Sparkle.csharp; 

public class Game : IDisposable {
    
    public static Game Instance;
    public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version!;

    private readonly GameSettings _settings;
    
    private readonly double _delay = 1.0 / 60.0;
    private double _timer;
    
    private bool _shouldClose;

    public Window Window { get; private set; }
    
    public Graphics Graphics { get; private set; }
    
    public ContentManager Content { get; private set; }
    
    public bool Headless { get; private set; }

    public Game(GameSettings settings, Scene scene) {
        Instance = this;
        this._settings = settings;
        this.Headless = settings.Headless;
        SceneManager.SetDefaultScene(scene);
    }

    public void Run() {
        Logger.Info($"Hello World! Sparkle [{Version}] start...");
        Logger.Info($"\tCPU: {SystemInfo.Cpu}");
        Logger.Info($"\tVIRTUAL MEMORY: {SystemInfo.VirtualMemorySize}MB");
        Logger.Info($"\tTHREADS: {SystemInfo.Threads}");
        Logger.Info($"\tOS: {SystemInfo.Os}");
        
        Logger.Debug("Initialize rayLib logger...");
        Logger.SetupRayLibLogger();

        if (!this.Headless) {
            Logger.Debug("Initialize window...");
            this.Window = new Window(this._settings.Size, this._settings.Title);

            Logger.Debug("Initialize content manager...");
            this.Content = new ContentManager(this._settings.ContentDirectory);
            
            Logger.Debug("Initialize settings...");
            this.Window.SetIcon(this.Content.Load<Image>(this._settings.IconPath));
            this.Window.SetStates(this._settings.WindowStates);
            this.SetTargetFps(this._settings.TargetFps);
        }

        Logger.Debug("Initialize graphics...");
        this.Graphics = new Graphics();

        this.Init();
        
        Logger.Debug("Run ticks...");
        while ((this.Headless && !this._shouldClose) || (!this.Headless && !this.Window.ShouldClose())) {
            this.Update();
            
            this._timer += Time.DeltaTime;
            while (this._timer >= this._delay) {
                this.FixedUpdate();
                this._timer -= this._delay;
            }

            if (!this.Headless) {
                this.Graphics.BeginDrawing();
                this.Graphics.ClearBackground(Color.SKYBLUE);
                this.Draw();
                this.Graphics.EndDrawing();
            }
        }
        
        this.OnClose();
    }
    
    protected virtual void Init() {
        SceneManager.Init();
    }

    protected virtual void Update() {
        SceneManager.Update();
    }

    protected virtual void FixedUpdate() {
        SceneManager.FixedUpdate();
    }
    
    protected virtual void Draw() {
        SceneManager.Draw();
    }
    
    protected virtual void OnClose() {
        Logger.Warn("Application shuts down!");
    }

    public void Close() {
        if (!this.Headless) {
            this.Window.Close();
        }

        this._shouldClose = true;
    }

    public int GetFps() {
        return Raylib.GetFPS();
    }

    public void SetTargetFps(int fps) {
        if (fps != 0) {
            Raylib.SetTargetFPS(fps);
        }
    }

    public unsafe void OpenURL(string url) {
        if (!this.Headless) {
            Raylib.OpenURL(url.ToUTF8Buffer().AsPointer());
        }
    }

    public virtual void Dispose() {
        if (!this.Headless) {
            this.Content.Dispose();
        }
        
        SceneManager.ActiveScene?.Dispose();
    }
}