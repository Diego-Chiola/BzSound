import React, { useEffect, useState } from "react";
import axios from "axios";
import { useSearchParams, useNavigate } from "react-router-dom";
import Button from "../common/components/Button";

const ConfirmEmail: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [status, setStatus] = useState<"loading" | "success" | "error">(
    "loading",
  );
  const [message, setMessage] = useState("");

  useEffect(() => {
    const confirmEmail = async () => {
      const email = searchParams.get("email");
      const token = searchParams.get("token");

      if (!email || !token) {
        setStatus("error");
        setMessage("Missing email or confirmation token.");
        return;
      }

      try {
        await axios.get("/api/auth/confirm-email", {
          params: {
            email: decodeURIComponent(email),
            token: decodeURIComponent(token),
          },
        });

        setStatus("success");
        setMessage("Email confirmed successfully! You can now log in.");
      } catch (err: any) {
        setStatus("error");
        setMessage(
          err.response?.data?.message ||
            "Failed to confirm email. The link may have expired.",
        );
      }
    };

    confirmEmail();
  }, [searchParams]);

  return (
    <div className="min-h-screen flex flex-col">
      <div className="flex-1 flex justify-center items-center p-5">
        <div className="bg-white p-10 rounded-2xl max-w-md w-full shadow-2xl">
          <h1 className="text-3xl font-bold text-gray-800 text-center mb-6">
            Email Confirmation
          </h1>

          {status === "loading" && (
            <div className="text-center">
              <div className="inline-block">
                <svg
                  className="animate-spin h-12 w-12 text-gray-600"
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
              </div>
              <p className="mt-4 text-gray-600">Confirming your email...</p>
            </div>
          )}

          {status === "success" && (
            <div className="text-center">
              <div className="text-6xl mb-4">✅</div>
              <p className="text-lg text-green-600 font-semibold mb-4">
                {message}
              </p>
              <Button
                variant="primary"
                onClick={() => navigate("/")}
                className="w-full"
              >
                Go to Login
              </Button>
            </div>
          )}

          {status === "error" && (
            <div className="text-center">
              <div className="text-6xl mb-4">❌</div>
              <p className="text-lg text-red-600 font-semibold mb-4">
                {message}
              </p>
              <Button
                variant="primary"
                onClick={() => navigate("/")}
                className="w-full"
              >
                Back to Home
              </Button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ConfirmEmail;
