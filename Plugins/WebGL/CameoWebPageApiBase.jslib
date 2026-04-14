mergeInto(LibraryManager.library, {
  CameoInjectWebPageApiBase: function (urlPtr) {
    var url = UTF8ToString(urlPtr);
    if (typeof window !== 'undefined' && url) {
      window.__CAMEO_API_BASE__ = url;
    }
  }
});
