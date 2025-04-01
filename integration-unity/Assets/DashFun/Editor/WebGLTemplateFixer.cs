using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class WebGLTemplateFixer : IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.WebGL)
            return;

        // 只处理 Unity 版本 < 2019
        var unityVersion = Application.unityVersion;
        if (IsVersionOrNewer(unityVersion, "2019.1.0"))
        {
            Debug.Log($"✅ Unity {unityVersion} 版本已原生支持 {{{{}}}} 模板，无需替换");
            return;
        }

        string outputDir = report.summary.outputPath;
        string indexPath = Path.Combine(outputDir, "index.html");
        string buildDir = Path.Combine(outputDir, "build");

        var index2018 = "Assets/DashFun/Editor/for2018/index.html";

        if (!File.Exists(indexPath) || !File.Exists(index2018))
        {
            Debug.LogWarning("Assets/DashFun/Editor/for2018/index.html not found");
            return;
        }

        var loader2018 = "Assets/DashFun/Editor/for2018/UnityLoader.js";
        if (!File.Exists(loader2018))
        {
            Debug.LogWarning("Assets/DashFun/Editor/for2018/UnityLoader.js not found");
            return;
        }

        var jsonPath = Directory.GetFiles(buildDir, "*.json")[0];
        var json = File.ReadAllText(jsonPath);

        var config = JsonUtility.FromJson<UnityWebGLBuildConfig>(json);

        var loaderFile = FindFile(buildDir, ".js"); // 获取唯一的 loader 文件
        File.Copy(loader2018, Path.Combine(buildDir, loaderFile), true); // 覆盖loader文件，去除提示
        var content = File.ReadAllText(index2018);

        // 替换模板变量
        content = ReplaceMustache(content, "PRODUCT_NAME", config.productName);
        content = ReplaceMustache(content, "COMPANY_NAME", config.companyName);
        content = ReplaceMustache(content, "PRODUCT_VERSION", PlayerSettings.bundleVersion);
        content = ReplaceMustache(content, "SPLASH_SCREEN_STYLE", config.splashScreenStyle);

        content = ReplaceMustache(content, "LOADER_FILENAME", loaderFile);
        content = ReplaceMustache(content, "DATA_FILENAME", config.dataUrl);
        content = ReplaceMustache(content, "CODE_FILENAME", config.wasmCodeUrl);
        content = ReplaceMustache(content, "FRAMEWORK_FILENAME", config.wasmFrameworkUrl);

        content = ReplaceMustache(content, "BUILD_JSON", "Build/" + Path.GetFileName(jsonPath));

        File.WriteAllText(indexPath, content);
        Debug.Log("✅ Unity 2018 WebGL Index.html 模板替换完成");
    }

    // 替换 {{{ VAR }}} 样式的变量
    private string ReplaceMustache(string text, string key, string value)
    {
        // 处理 {{{ VAR.toLowerCase() }}}
        string patternLower = @"\{\{\{\s*" + key + @"\.toLowerCase\(\)\s*\}\}\}";
        text = Regex.Replace(text, patternLower, value.ToLower());

        // 处理 {{{ VAR }}}
        string patternRaw = @"\{\{\{\s*" + key + @"\s*\}\}\}";
        text = Regex.Replace(text, patternRaw, value);

        return text;
    }


    private string FindFile(string dir, string endsWith)
    {
        var files = Directory.GetFiles(dir);
        foreach (var file in files)
        {
            if (file.EndsWith(endsWith))
            {
                return Path.GetFileName(file);
            }
        }

        Debug.LogWarning($"⚠️ 未找到以 {endsWith} 结尾的文件！");
        return "";
    }

    // 比较版本号，返回是否当前版本 >= 目标版本
    private bool IsVersionOrNewer(string current, string target)
    {
        System.Version v1 = new System.Version(current.Split('f')[0]);
        System.Version v2 = new System.Version(target);
        return v1 >= v2;
    }
}