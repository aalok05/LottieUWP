﻿using Windows.UI.Xaml.Media.Imaging;

namespace LottieUWP
{
    /// <summary>
    /// Delegate to handle the loading of bitmaps that are not packaged in the assets of your app.
    /// </summary>
    public interface IImageAssetDelegate
    {
        BitmapSource FetchBitmap(LottieImageAsset asset);
    }
}