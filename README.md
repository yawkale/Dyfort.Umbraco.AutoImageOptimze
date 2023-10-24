# Dyfort.Umbraco.AutoImageOptimize

With this powerful tool, you can make your images look incredible while also improving your website's performance. Say goodbye to slow-loading images and hello to a lightning-fast browsing experience!

Our Automatic Image Optimization product is designed to automatically enhance and optimize your images with just a few clicks. It uses clever algorithms and advanced techniques to reduce the file size of your images without compromising their quality.

This means that your website will load faster, allowing your visitors to see your images in an instant. Whether you're showcasing your products, sharing memories with friends, or displaying stunning artwork, our tool ensures that your images are displayed at their best.

## Install Package to your project

```
    PM> Install-Package Dyfort.Umbraco.AutoImageOptimize
```

After installation, don't forget to clean the cache folder, which is located at umbraco\Data\TEMP\MediaCache

Settings and defaults

```
 "AutoImageOptimizer": {
    "Quality": "92",
    "AllowedExtentions": [
      ".png",
      ".jpg",
      ".jpeg"
    ],
    "ExcludedFolderPaths":[
      "/umbraco/assets/"
    ],
    "Enabled": "true"
  }
```
