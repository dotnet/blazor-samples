import nodeResolve from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';
import babel from '@rollup/plugin-babel';
import replace from '@rollup/plugin-replace';
import legacy from '@rollup/plugin-legacy';
import files from 'rollup-plugin-import-file';

export default {
    input: 'src/index.js',
    output: {
        dir: 'public',
        format: 'esm',
        inlineDynamicImports: true,
    },
    plugins: [
        legacy({
            "../blazor/bin/Release/net10.0/publish/wwwroot/_framework/blazor.webassembly.js": "window.Blazor"
        }),
        nodeResolve({
            extensions: ['.js']
        }),
        babel({
            babelHelpers: 'bundled',
            presets: ['@babel/preset-react'],
            extensions: ['.js'],
            generatorOpts: {
                // Increase the size limit from 500KB to 10MB
                compact: true,
                retainLines: true,
                maxSize: 10000000
            }
        }),
        files({
            output: 'public',
            extensions: /\.(wasm|dat)$/,
            hash: true,
        }),
        commonjs(),
        replace({
            preventAssignment: false,
            'process.env.NODE_ENV': '"production"'
        }),
    ]
}
