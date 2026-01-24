import React from 'react';

interface HeaderProps {
  isAuthenticated: boolean;
  onLogin: () => void;
  onLogout: () => void;
}

const Header: React.FC<HeaderProps> = ({ isAuthenticated, onLogin, onLogout }) => {
  return (
    <header className="bg-white bg-opacity-95 shadow-lg sticky top-0 z-50">
      <div className="max-w-7xl mx-auto px-5 py-4 flex justify-between items-center">
        <h1 className="text-3xl font-bold text-primary m-0">🎵 BzSound</h1>
        <nav className="flex gap-2">
          {isAuthenticated ? (
            <>
              <button className="px-5 py-2 border-none rounded-lg text-base cursor-pointer transition-all bg-transparent text-gray-800 hover:bg-primary hover:bg-opacity-10">
                My Files
              </button>
              <button 
                className="px-5 py-2 border-none rounded-lg text-base cursor-pointer transition-all bg-transparent text-gray-800 hover:bg-primary hover:bg-opacity-10"
                onClick={onLogout}
              >
                Logout
              </button>
            </>
          ) : (
            <button 
              className="px-5 py-2 border-none rounded-lg text-base cursor-pointer transition-all bg-primary text-white hover:bg-primary-dark hover:-translate-y-0.5 hover:shadow-lg"
              onClick={onLogin}
            >
              Login / Register
            </button>
          )}
        </nav>
      </div>
    </header>
  );
};

export default Header;
