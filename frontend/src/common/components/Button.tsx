import React from "react";

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: "primary" | "secondary" | "accent" | "outline" | "link";
  size?: "sm" | "md" | "lg";
  isLoading?: boolean;
  children: React.ReactNode;
}

const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  (
    {
      variant = "primary",
      size = "md",
      isLoading = false,
      className = "",
      disabled,
      children,
      ...props
    },
    ref,
  ) => {
    // Base styles
    const baseStyles =
      "font-semibold transition-all duration-200 focus:outline-none disabled:opacity-50 disabled:cursor-not-allowed";

    // Size variants (not applied to link variant)
    const sizeStyles = {
      sm: "px-3 py-1.5 text-sm",
      md: "px-4 py-2.5 text-base",
      lg: "px-6 py-3 text-lg",
    };

    // Color variants
    const variantStyles = {
      primary:
        "bg-primary hover:bg-primary-dark hover:scale-[1.02] text-white rounded-lg focus:ring-2 focus:ring-offset-2 focus:ring-primary",
      secondary:
        "bg-secondary hover:bg-secondary hover:scale-[1.02] text-white rounded-lg focus:ring-2 focus:ring-offset-2 focus:ring-secondary",
      accent:
        "bg-accent hover:bg-yellow-500 text-black rounded-lg focus:ring-2 focus:ring-offset-2 focus:ring-accent",
      outline:
        "border-2 border-primary text-primary hover:bg-primary hover:text-white hover:scale-[1.02] rounded-lg focus:ring-2 focus:ring-offset-2 focus:ring-primary",
      link: "text-secondary font-semibold underline hover:text-primary-dark bg-transparent border-none p-0",
    };

    const applySizeStyles = variant !== "link";
    const combinedClassName = `${baseStyles} ${applySizeStyles ? sizeStyles[size] : ""} ${variantStyles[variant]} ${className}`;

    return (
      <button
        ref={ref}
        disabled={disabled || isLoading}
        className={combinedClassName}
        {...props}
      >
        {isLoading ? (
          <span className="flex items-center gap-2">
            <svg
              className="animate-spin h-5 w-5"
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
            >
              <circle
                className="opacity-25"
                cx="12"
                cy="12"
                r="10"
                stroke="currentColor"
                strokeWidth="4"
              ></circle>
              <path
                className="opacity-75"
                fill="currentColor"
                d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
              ></path>
            </svg>
            Loading...
          </span>
        ) : (
          children
        )}
      </button>
    );
  },
);

Button.displayName = "Button";

export default Button;
