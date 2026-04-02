/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{js,jsx,ts,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#FBBF24',
          dark: '#D97706',
        },
        secondary: {
          DEFAULT: '#1F2937',
          dark: '#111827',
        }
      },
      backgroundColor: {
        'primary': '#FBBF24',
        'secondary': '#1F2937',
        'secondary-dark': '#111827',
      },
      textColor: {
        'primary': '#FFFFFF',
        'secondary': '#1F2937',
      },
    },
  },
  plugins: [],
}
