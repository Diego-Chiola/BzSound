import React, { useState } from "react";
import axios from "axios";
import Button from "../common/components/Button";
import { AuthResponse } from "../types";
import { Icon } from "react-icons-kit";
import { eyeOff } from "react-icons-kit/feather/eyeOff";
import { eye } from "react-icons-kit/feather/eye";

interface AuthProps {
  onLogin: (token: string, userId: string) => void;
  onClose: () => void;
}

const Auth: React.FC<AuthProps> = ({ onLogin, onClose }) => {
  const [isLogin, setIsLogin] = useState(true);
  const [isPasswordVisible, setIsPasswordVisible] = useState(false);
  const [showRegistrationSuccess, setShowRegistrationSuccess] = useState(false);
  const [formData, setFormData] = useState({
    email: "",
    password: "",
  });
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  // Password validation rules
  const passwordValidation = {
    minLength: formData.password.length >= 8,
    hasDigit: /\d/.test(formData.password),
    hasUppercase: /[A-Z]/.test(formData.password),
    hasNonAlphanumeric: /[^a-zA-Z0-9]/.test(formData.password),
  };

  const isPasswordValid =
    isLogin || Object.values(passwordValidation).every(Boolean);
  const isFormValid =
    formData.email && formData.password && (isLogin || isPasswordValid);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      if (!isLogin) {
        // Registration flow
        const registerPayload = {
          username: formData.email.split("@")[0],
          email: formData.email,
          password: formData.password,
        };
        await axios.post("/api/auth/register", registerPayload);
        setShowRegistrationSuccess(true);
        setFormData({ email: "", password: "" });
      } else {
        // Login flow
        const response = await axios.post<AuthResponse>("/api/auth/login", {
          email: formData.email,
          password: formData.password,
        });

        // Decode JWT to get userId
        const tokenPayload = JSON.parse(
          atob(response.data.token.split(".")[1]),
        );
        const userId = tokenPayload.nameid || tokenPayload.sub;

        onLogin(response.data.token, userId);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || "An error occurred");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-70 flex justify-center items-center z-50">
      <div
        className="bg-white md:p-10 p-5 rounded-2xl max-w-md w-11/12 relative shadow-2xl"
        onClick={(e) => e.stopPropagation()}
      >
        <button
          className="absolute top-4 right-4 bg-transparent border-none text-4xl cursor-pointer text-gray-400 leading-none hover:text-gray-800 p-0"
          onClick={onClose}
        >
          &times;
        </button>

        {error && (
          <div className=" text-red-700 p-3 rounded-lg mb-5 text-center">
            {error}
          </div>
        )}

        {showRegistrationSuccess && (
          <div className=" text-gray-900 p-4 md:p-0 rounded-lg text-center">
            <p className="font-semibold mb-2">Registration Successful!</p>
            <p className="text-base">
              Please confirm your email to complete the registration. Check your
              inbox for a confirmation link.
            </p>
            <Button
              variant="link"
              onClick={() => {
                setShowRegistrationSuccess(false);
                setIsLogin(true);
              }}
              className="mt-3"
            >
              Back to Login
            </Button>
          </div>
        )}

        {!showRegistrationSuccess && (
          <form onSubmit={handleSubmit} className="mt-3">
            <div className="mb-5">
              <label className="block mb-1 font-medium text-gray-800">
                Email
              </label>
              <input
                type="email"
                name="email"
                value={formData.email}
                onChange={handleChange}
                required
                className="w-full p-3 border-2 border-gray-300 rounded-lg text-base text-gray-900 transition-colors focus:outline-none focus:border-primary"
              />
            </div>

            <div className="mb-5">
              <label className="block mb-1 font-medium text-gray-800">
                Password
              </label>
              <div className="relative">
                <input
                  type={isPasswordVisible ? "text" : "password"}
                  name="password"
                  value={formData.password}
                  onChange={handleChange}
                  required
                  className="w-full p-3 border-2 border-gray-300 rounded-lg text-base text-gray-900 transition-colors focus:outline-none focus:border-primary pr-10"
                />
                <button
                  type="button"
                  onClick={() => setIsPasswordVisible(!isPasswordVisible)}
                  className="absolute right-3 top-3 text-gray-600 hover:text-gray-900 bg-transparent border-none cursor-pointer p-0"
                >
                  {isPasswordVisible ? (
                    <Icon icon={eye} />
                  ) : (
                    <Icon icon={eyeOff} />
                  )}
                </button>
              </div>

              {!isLogin && (
                <div className="mt-3 text-sm space-y-1">
                  <div
                    className={`flex items-center gap-2 ${passwordValidation.minLength ? "text-green-600" : "text-red-600"}`}
                  >
                    <span>{passwordValidation.minLength ? "✓" : "✗"}</span>
                    At least 8 characters
                  </div>
                  <div
                    className={`flex items-center gap-2 ${passwordValidation.hasDigit ? "text-green-600" : "text-red-600"}`}
                  >
                    <span>{passwordValidation.hasDigit ? "✓" : "✗"}</span>
                    At least one digit (0-9)
                  </div>
                  <div
                    className={`flex items-center gap-2 ${passwordValidation.hasUppercase ? "text-green-600" : "text-red-600"}`}
                  >
                    <span>{passwordValidation.hasUppercase ? "✓" : "✗"}</span>
                    At least one uppercase (A-Z)
                  </div>
                  <div
                    className={`flex items-center gap-2 ${passwordValidation.hasNonAlphanumeric ? "text-green-600" : "text-red-600"}`}
                  >
                    <span>
                      {passwordValidation.hasNonAlphanumeric ? "✓" : "✗"}
                    </span>
                    At least one special character (!@#$%^&*)
                  </div>
                </div>
              )}
            </div>

            <Button
              type="submit"
              variant="primary"
              size="lg"
              disabled={loading || !isFormValid}
              isLoading={loading}
              className="w-full"
            >
              {isLogin ? "Login" : "Register"}
            </Button>
          </form>
        )}
        {!showRegistrationSuccess && (
          <p className="text-center mt-5 text-gray-600">
            <>
              {isLogin
                ? "Don't have an account? "
                : "Already have an account? "}
              <Button variant="link" onClick={() => setIsLogin(!isLogin)}>
                {isLogin ? "Register" : "Login"}
              </Button>
            </>
          </p>
        )}
      </div>
    </div>
  );
};

export default Auth;
