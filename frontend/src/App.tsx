import React, { useState } from "react";
import AudioEditor from "./components/AudioEditor";
import Header from "./components/Header";
import Auth from "./components/Auth";

const App: React.FC = () => {
  const [token, setToken] = useState<string | null>(
    localStorage.getItem("token"),
  );
  const [userId, setUserId] = useState<string | null>(
    localStorage.getItem("userId"),
  );
  const [showAuth, setShowAuth] = useState(false);

  const handleLogin = (newToken: string, newUserId: string) => {
    localStorage.setItem("token", newToken);
    localStorage.setItem("userId", newUserId);
    setToken(newToken);
    setUserId(newUserId);
    setShowAuth(false);
  };

  const handleLogout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("userId");
    setToken(null);
    setUserId(null);
  };

  return (
    <div className="min-h-screen flex flex-col">
      <Header
        isAuthenticated={!!token}
        onLogin={() => setShowAuth(true)}
        onLogout={handleLogout}
      />

      {showAuth && !token && (
        <Auth onLogin={handleLogin} onClose={() => setShowAuth(false)} />
      )}

      <main className="flex-1 p-5 max-w-7xl w-full mx-auto">
        <AudioEditor token={token} userId={userId} />
      </main>

      <footer className="bg-black bg-opacity-20 text-white text-center p-5 mt-auto">
        <p className="m-0 text-sm">
          &copy; 2026 BzSound. Audio manipulation made easy.
        </p>
      </footer>
    </div>
  );
};

export default App;
