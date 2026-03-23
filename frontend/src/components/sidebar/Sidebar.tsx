import useSidebar from "../../hooks/useSidebar";
import SidebarNav from "./SidebarNav";

const BrandIcon = () => (
  <svg width="24" height="24" viewBox="0 0 33 33" fill="none" xmlns="http://www.w3.org/2000/svg" style={{ flexShrink: 0 }}>
    <rect x="5" y="5" width="22" height="22" rx="4" fill="#4F46E5" transform="rotate(45 16 16)" />
  </svg>
);

const Sidebar = () => {
  const { isOpen } = useSidebar();

  return (
    <nav className={`sidebar ${!isOpen ? "collapsed" : ""}`}>
      <div className="sidebar-content">
          <a className="sidebar-brand" href="/" style={{ display: "flex", alignItems: "center", justifyContent: "center", gap: "0.15rem" }}>
            <BrandIcon />
            <span style={{ fontWeight: 600, fontSize: "1.15rem", whiteSpace: "nowrap" }}>ClientControl</span>
          </a>
          <SidebarNav />
      </div>
    </nav>
  );
};

export default Sidebar;
