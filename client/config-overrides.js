const webpack = require('webpack');
const { override } = require('customize-cra');

module.exports = override((config) => {
    config.resolve.fallback = {
        path: require.resolve('path-browserify'),
        os: require.resolve('os-browserify/browser'),
        crypto: require.resolve('crypto-browserify'),
        buffer: require.resolve('buffer/'),
        stream: require.resolve('stream-browserify'),
        process: require.resolve('process/browser'),
        vm: require.resolve('vm-browserify'),
    };
    config.plugins = (config.plugins || []).concat([
        new webpack.ProvidePlugin({
            process: 'process/browser',
        }),
    ]);
    return config;
});