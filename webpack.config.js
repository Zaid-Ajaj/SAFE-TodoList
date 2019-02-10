var path = require("path");
var webpack = require("webpack");

function resolve(filePath) {
  return path.join(__dirname, filePath)
}

var babelOptions = {
  presets: [
    ["@babel/preset-env", {
        "modules": false,
        "useBuiltIns": "usage",
    }]
  ]
};

var port = process.env.SUAVE_FABLE_PORT || "8085";

module.exports = function (evn, argv) {
  var mode = argv.mode || "development";
  var isProduction = mode === "production";
  console.log("Webpack mode: " + mode);

  return {
    devtool: "source-map",
    entry: resolve('./src/Client/Client.fsproj'),
    output: {
      path: resolve('./src/Client/dist'),
      filename: "bundle.js"
    },
    resolve: {
      modules: [ resolve("./node_modules")]
    },
    devServer: {
      proxy: {
        '/api/*': {
          target: 'http://localhost:' + port,
          changeOrigin: true
        }
      },
      hot: true,
      inline: true,
      contentBase: resolve('./src/Client/dist'),
    },
    module: {
      rules: [
        {
          test: /\.fs(x|proj)?$/,
          use: "fable-loader"
        },
        {
          test: /\.js$/,
          exclude: /node_modules/,
          use: {
            loader: 'babel-loader',
            options: babelOptions
          },
        }
      ]
    },
    plugins : isProduction ? [] : [
      new webpack.HotModuleReplacementPlugin(),
      new webpack.NamedModulesPlugin()
    ]
  }
};
