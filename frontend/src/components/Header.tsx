import React from "react";
import Button from "../common/components/Button";

interface HeaderProps {
  isAuthenticated: boolean;
  onLogin: () => void;
  onLogout: () => void;
}

const Header: React.FC<HeaderProps> = ({
  isAuthenticated,
  onLogin,
  onLogout,
}) => {
  return (
    <header className="bg-primary bg-opacity-95 shadow-lg sticky top-0 z-50">
      <div className="max-w-8xl mx-auto px-5 py-4 flex justify-between items-center">
        <img src="/logo.png" alt="BzSound logo" className="h-12 w-auto" />
        <nav className="flex gap-2">
          {isAuthenticated ? (
            <>
              <Button variant="secondary">My Files</Button>
              <Button variant="secondary" onClick={onLogout}>
                Logout
              </Button>
            </>
          ) : (
            <Button variant="secondary" onClick={onLogin}>
              Login / Register
            </Button>
          )}
        </nav>
      </div>
    </header>
  );
};

export default Header;
