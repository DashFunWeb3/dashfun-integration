[System.Serializable]
public class UnityWebGLBuildConfig
{
    public string companyName;
    public string productName;
    public string dataUrl;
    public string wasmCodeUrl;
    public string wasmFrameworkUrl;
    public int TOTAL_MEMORY;
    public string[] graphicsAPI;
    public WebGLContextAttributes webglContextAttributes;
    public string splashScreenStyle;
    public string backgroundColor;
    public CacheControl cacheControl;
}

[System.Serializable]
public class WebGLContextAttributes
{
    public bool preserveDrawingBuffer;
}

[System.Serializable]
public class CacheControl
{
    public string @default;
}