import React, { useState } from "react";
import AdminUsers from "./AdminUsers";
import AdminChallenges from "./AdminChallenges";
import AdminCategories from "./AdminCategories";

function Admin() {
  const [section, setSection] = useState("users");

  return (
    <div style={{ maxWidth: 700, margin: "3rem auto" }}>
      <h1>Admin Dashboard</h1>
      <div style={{ display: "flex", gap: 12, marginBottom: 24 }}>
        <button onClick={() => setSection("users")}>Users</button>
        <button onClick={() => setSection("challenges")}>Challenges</button>
        <button onClick={() => setSection("categories")}>Categories</button>
      </div>
      {section === "users" && <AdminUsers />}
      {section === "challenges" && <AdminChallenges />}
      {section === "categories" && <AdminCategories />}
    </div>
  );
}

export default Admin;