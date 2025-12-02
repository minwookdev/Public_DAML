using UnityEngine;
using System.IO;
using CoffeeCat;
using Sirenix.OdinInspector;

public class PngResizer : MonoBehaviour
{
    [FolderPath] public string inputFolderPath;  // 원본 PNG 파일 경로
    [FolderPath] public string outputFolderPath; // 저장할 PNG 파일 경로
    public int newWidth = 256;                   // 새로운 너비
    public int newHeight = 256;                  // 새로운 높이

    [Button("Process")]
    private void Process() {
        if (!Directory.Exists(inputFolderPath) || !Directory.Exists(outputFolderPath)) {
            CatLog.WLog("Invalid Directory Path");
            return;
        }
        
        // .meta 파일을 제외한 .png 파일들을 가져온다.
        string[] files = Directory.GetFiles(inputFolderPath, "*.png");
        foreach (string file in files) {
            string fileName = Path.GetFileName(file);
            string path = Path.Combine(inputFolderPath, fileName);
            if (File.Exists(path))
            {
                // 파일 읽기
                byte[] fileData = File.ReadAllBytes(path);

                // Texture2D 생성 및 PNG 로드
                Texture2D originalTexture = new Texture2D(2, 2);
                originalTexture.LoadImage(fileData);

                // 리사이즈 수행
                Texture2D resizedTexture = ResizeTexture(originalTexture, newWidth, newHeight);

                // PNG로 저장
                var outputPath = Path.Combine(outputFolderPath, fileName);
                byte[] resizedPng = resizedTexture.EncodeToPNG();
                File.WriteAllBytes(outputPath, resizedPng);

                Debug.Log($"이미지가 성공적으로 {outputPath}에 저장되었습니다.");
            }
            else
            {
                Debug.LogError("입력 파일 경로가 존재하지 않습니다.");
            }
        }
        

    }

    private Texture2D ResizeTexture(Texture2D originalTexture, int width, int height)
    {
        // 새로운 Texture2D 생성
        Texture2D resizedTexture = new Texture2D(width, height, originalTexture.format, false);

        // 리샘플링 수행
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 원본 텍스처의 좌표를 비율에 맞게 매핑
                float u = (float)x / (width - 1);
                float v = (float)y / (height - 1);
                Color color = originalTexture.GetPixelBilinear(u, v);
                resizedTexture.SetPixel(x, y, color);
            }
        }

        resizedTexture.Apply();
        return resizedTexture;
    }
}