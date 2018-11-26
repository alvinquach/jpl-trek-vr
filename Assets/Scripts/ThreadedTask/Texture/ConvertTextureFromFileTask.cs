using App.Texture.Models;
using App.ThreadedTask;

public class ConvertTextureFromFileTask : ThreadedTask<float, byte[]> {

    private string _filepath;

    private TextureCompressionFormat _textureFormat;

    private float _progress = 0.0f;

    public int TextureWidth { get; private set; }

    public int TextureHeight { get; private set; }

    public ConvertTextureFromFileTask(string filepath, TextureCompressionFormat textureFormat = TextureCompressionFormat.DXT5) {
        _filepath = filepath;
        _textureFormat = textureFormat;
    }

    public override float GetProgress() {
        return _progress;
    }

    protected sealed override byte[] Task() {

        RGBAImage srcImage;

        // TODO Support other file types.
        // TODO Check if path is valid.
        using (TiffWrapper tiff = new TiffWrapper(_filepath)) {
            srcImage = tiff.ToRGBAImage();
        }

        TextureWidth = srcImage.Width;
        TextureHeight = srcImage.Height;

        return TextureToolUtils.ImageToTexture(srcImage, _textureFormat);
    }

}
