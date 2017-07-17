module.exports = function (grunt)
{
	var reloadTokens = ['Chat.RestAPI/LiveReloadOnSuccessfulBuild.token', 'Chat.UI/LiveReloadOnSuccessfulBuild.token'];

	grunt.initConfig
	({
		watch: {
			files: reloadTokens,
			scripts: {
				files: ['Chat.UI/**/*.js'],
				tasks: ['default']
			},
			css: {
				files: ['Chat.UI/**/*.css'],
				tasks: ['default']
			},
			src: {
				files: ['Chat.UI/**/*.html'],
				tasks: ['default']
			},
			options: {
				livereload: true
			}
		}
	});

	grunt.loadNpmTasks('grunt-contrib-watch');
	grunt.registerTask('default', ['grunt-contrib-watch']);
};