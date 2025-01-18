/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./**/*.razor",
    "./wwwroot/index.html",
    "./**/*.cshtml",
    "./**/*.html"
  ],
  theme: {
    extend: {},
  },
  plugins: [
    require('cssnano')({
      preset: 'default',
    }),
  ],
};
