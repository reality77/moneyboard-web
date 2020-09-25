/// <binding AfterBuild='default' Clean='clean' />
/*
This file is the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/
var gulp = require('gulp');
var sass = require('gulp-sass');
var del = require('del');
var minify = require('gulp-minify');
var uglifycss = require('gulp-uglifycss');

var nodeRoot = './node_modules/';
var targetPath = './wwwroot/';
var targetLibPath = targetPath + 'lib/';

var config = {
    "css": {
      "src": [
          "Style/*.scss"
      ],
      "dest": targetPath + "/css/"
    },
    "scripts": {
      "src": [
        "Scripts/**/*.js"
      ],
        "dest": targetPath + "/js/"
    },
  }
  

gulp.task('clean', function () {
    return del([targetPath + '/**/*']);
});

gulp.task('css', function() {
    return gulp.src(config.css.src)
        /*.pipe(glob())
        .pipe(plumber({
            errorHandler: function (error) {
                notify.onError({
                    title:    "Gulp",
                    subtitle: "Failure!",
                    message:  "Error: <%= error.message %>",
                    sound:    "Beep"
                }) (error);
                this.emit('end');
            }}))
        .pipe(sourcemaps.init())*/
        .pipe(sass({
            style: 'compressed',
            errLogToConsole: true
        })).pipe(uglifycss())
        //.pipe(sourcemaps.write('./'))
        .pipe(gulp.dest(config.css.dest));
});

gulp.task('fonts', function () {
    return gulp.src("./Style/douane-design-system/fonts/**/*.*").pipe(gulp.dest(targetPath + "/fonts/"));
});

gulp.task('icons', function () {
    return gulp.src("./Style/douane-design-system/icon/**/*.*").pipe(gulp.dest(targetPath + "/icon/"));
});

gulp.task('images', function () {
    return gulp.src("./Style/images/**/*.*").pipe(gulp.dest(targetPath + "/images/"));
});

gulp.task('bootstrap-js', function () {
    return gulp.src(nodeRoot + "bootstrap/dist/**/*.js*").pipe(gulp.dest(targetLibPath + "/bootstrap/dist/"));
});

gulp.task('bootstrap-select-js', function () {
    return gulp.src(nodeRoot + "bootstrap-select/dist/**/*.js*").pipe(gulp.dest(targetLibPath + "/bootstrap-select/dist/"));
});

gulp.task('jquery', function () {
    return gulp.src(nodeRoot + "jquery/dist/jquery.*").pipe(gulp.dest(targetLibPath + "/jquery/dist/"));
});

gulp.task('jquery-ui', function () {
    return gulp.src(nodeRoot + "jquery-ui-dist/*.*").pipe(gulp.dest(targetLibPath + "/jquery-ui/dist/"));
});

gulp.task('jquery-validation', function () {
    return gulp.src(nodeRoot + "jquery-validation/dist/*.*").pipe(gulp.dest(targetLibPath + "/jquery-validation/dist/"));
});

gulp.task('jquery-validation-unobtrusive', function () {
    return gulp.src(nodeRoot + "jquery-validation-unobtrusive/dist/*.*").pipe(gulp.dest(targetLibPath + "/jquery-validation-unobtrusive/dist/"));
});


gulp.task('scripts', function() {
    return  gulp.src(config.scripts.src)
    .pipe(minify())
    .pipe(gulp.dest(config.scripts.dest))
});


gulp.task('default', gulp.series('clean', 'css', 'fonts', 'icons', 'images', 'bootstrap-js', 'bootstrap-select-js', 'jquery', 'jquery-ui', 'jquery-validation', 'jquery-validation-unobtrusive', 'scripts'));

// Watch task.
gulp.task('watch', function() {
    gulp.watch(config.css.src, gulp.series('css'));
    //gulp.watch(config.images.src, ['images']);
});
