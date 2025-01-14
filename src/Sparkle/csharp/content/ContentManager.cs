using Raylib_cs;
using Sparkle.csharp.content.processor;

namespace Sparkle.csharp.content; 

public class ContentManager : IDisposable {

    private readonly string _contentDirectory;

    private readonly List<object> _content;
    private readonly Dictionary<Type, IContentProcessor> _processors;

    public ContentManager(string directory) {
        this._contentDirectory = $"{directory}/";
        this._content = new List<object>();
        
        this._processors = new Dictionary<Type, IContentProcessor>();
        this.AddProcessors(typeof(Font), new FontProcessor());
        this.AddProcessors(typeof(Image), new ImageProcessor());
        this.AddProcessors(typeof(Model), new ModelProcessor());
        this.AddProcessors(typeof(Sound), new SoundProcessor());
        this.AddProcessors(typeof(Texture2D), new TextureProcessor());
    }

    public void AddProcessors(Type type, IContentProcessor processor) {
        this._processors.Add(type, processor);
    }

    public IContentProcessor TryGetProcessor(Type type) {
        if (!this._processors.TryGetValue(type, out IContentProcessor? processor)) {
            Logger.Error($"Unable to locate ContentProcessor for type [{type}]!");
        }

        return processor!;
    }

    public T Load<T>(string path) {
        T content = (T) this.TryGetProcessor(typeof(T)).Load(this._contentDirectory + path);

        this._content.Add(content!);
        return content;
    }
    
    public void Unload<T>(T content) {
        if (this._content.Contains(content!)) {
            this.TryGetProcessor(typeof(T)).Unload(content!);
            this._content.Remove(content!);
        }
        else {
            Logger.Warn($"Unable to unload content for the specified type {typeof(T)}!");
        }
    }
    
    public void Dispose() {
        foreach (object content in this._content.ToList()) {
            this.TryGetProcessor(content.GetType()).Unload(content);
            this._content.Remove(content);
        }
    }
}