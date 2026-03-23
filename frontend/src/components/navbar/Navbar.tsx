import { Navbar, Nav } from "react-bootstrap";
import { Moon, Sun } from "react-feather";
import useSidebar from "../../hooks/useSidebar";
import useTheme from "../../hooks/useTheme";
import { THEME } from "../../constants";
import NavbarUser from "./NavbarUser";

const NavbarComponent = () => {
  const { isOpen, setIsOpen } = useSidebar();
  const { theme, setTheme } = useTheme();

  const isDark = theme === THEME.DARK;

  return (
    <Navbar variant="light" expand className="navbar-bg">
      <span
        className="sidebar-toggle d-flex"
        onClick={() => {
          setIsOpen(!isOpen);
        }}
      >
        <i className="hamburger align-self-center" />
      </span>

      <Navbar.Collapse>
        <Nav className="navbar-align">
            <a
              className="nav-link nav-icon d-flex align-items-center"
              role="button"
              onClick={() => setTheme(isDark ? THEME.DEFAULT : THEME.DARK)}
              title={isDark ? "Tema claro" : "Tema escuro"}
              style={{ cursor: "pointer" }}
            >
              {isDark ? <Sun size={18} /> : <Moon size={18} />}
            </a>
            <NavbarUser />
        </Nav>
      </Navbar.Collapse>
    </Navbar>
  );
};

export default NavbarComponent;
