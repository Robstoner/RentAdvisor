import React, { ReactNode } from 'react';
// import Sidebar from '../components/Sidebar';

interface MainLayoutProps {
    children: ReactNode;
    // topics: any[];
}

const MainLayout: React.FC<MainLayoutProps> = ({ children }) => {
    return (
        <div className="app-container">
            {/* <aside className="sidebar">
                <Sidebar topics={topics} />
            </aside> */}
            <main className="main-content">
                {children}
            </main>
        </div>
    );
};

export default MainLayout;