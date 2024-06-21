const { RawSource } = require("webpack").sources;

exports.CSSPresencePlugin = class CSSPresencePlugin {
  apply(compiler) {
    compiler.hooks.compilation.tap("CSSPresencePlugin", (compilation) => {
      compilation.hooks.processAssets.tap(
        {
          name: "CSSPresencePlugin",
          stage: compilation.PROCESS_ASSETS_STAGE_ADDITIONS,
        },
        () => {
          const cssFiles = Object.keys(compilation.assets).filter((asset) =>
            asset.endsWith(".css")
          );
          const hasCSS = cssFiles.length > 0;

          // Inject the `hasCSS` export into the main module source
          for (const chunk of compilation.chunks) {
            for (const file of chunk.files) {
              if (file.endsWith(".mjs")) {
                const asset = compilation.getAsset(file);
                const source = asset.source.source();
                const updatedSource = source.replace(
                  "export {",
                  `const hasCSS = ${hasCSS}; export { hasCSS, `
                );

                // Generate a new source map for the modified source
                const newSourceAndMap = {
                  sources: [file],
                  mappings: "",
                  file,
                  sourceRoot: "",
                  sourcesContent: [updatedSource],
                };

                compilation.updateAsset(
                  file,
                  new RawSource(updatedSource, newSourceAndMap)
                );
              }
            }
          }
        }
      );
    });
  }
};
