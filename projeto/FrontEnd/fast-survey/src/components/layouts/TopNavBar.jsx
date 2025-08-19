import React from "react";
import { useNavigate } from "react-router-dom";
import "./topnavbar.css";
import { ArrowLeft } from "lucide-react";

const TopNavbar = () => {
  const navigate = useNavigate();

  return (
    <header className="top-navbar" role="banner">
      <h1 className="logo-title">FastSurvey</h1>
      <button
        className="back-home"
        onClick={() => navigate("/home")}
        aria-label="Voltar para Home"
      >
        <ArrowLeft size={18} style={{ marginRight: "6px" }} />
        Voltar para Home
      </button>
    </header>
  );
};

export default TopNavbar;
