/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,jsx}'],
  theme: {
    extend: {
      colors: {
        brand: {
          50: '#eef4ff',
          500: '#4f6bed',
          600: '#3b53c4',
          700: '#2f42a0'
        }
      }
    }
  },
  plugins: []
}
