/// <binding AfterBuild='CopySignalR, CopyPnotify' />
var gulp = require("gulp");
// folders

var folders = {
    root: "./wwwroot/"
};
gulp.task("CopySignalR",
    function() {
        gulp.src("./node_modules/@aspnet/signalr/dist/browser/*")
            .pipe(gulp.dest(folders.root + "lib/signalr"));
    });
gulp.task("CopyPnotify",
    function () {
        gulp.src("./node_modules/pnotify/dist/*")
            .pipe(gulp.dest(folders.root + "lib/pnotify"));
    });
