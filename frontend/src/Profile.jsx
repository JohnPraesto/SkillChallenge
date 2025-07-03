import React, { useEffect, useState } from "react";
import { useAuth } from "./AuthContext";
import { useToast } from "./ToastContext";
import { LoadingSkeleton } from "./LoadingSkeleton";

function Profile() {
  const { user, login, logout } = useAuth();
  const { showSuccess, showError } = useToast();
  const [userData, setUserData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [activeForm, setActiveForm] = useState(null);
  const [formData, setFormData] = useState({
    username: "",
    currentPassword: "",
    newPassword: "",
    pictureFile: ""
  });
  const apiUrl = import.meta.env.VITE_API_URL;

  useEffect(() => {
    console.log("AuthContext user:", user);
  }, [user]);

  useEffect(() => {
    if (!user?.id) return;
    
    fetch(`${apiUrl}/users/id/${user.id}`)
      .then(res => res.ok ? res.json() : Promise.reject("Failed to fetch user"))
      .then(data => {
        setUserData(data);
        setLoading(false);
      })
      .catch(err => {
        showError("Failed to load profile");
        setLoading(false);
      });
  }, [user?.id, showError]);

  const handleUpdate = async (field, value) => {
    try {
      const res = await fetch(`${apiUrl}/users/${user.id}`, {
        method: "PUT",
        headers: { 
          "Content-Type": "application/json",
          "Authorization": `Bearer ${localStorage.getItem("token")}`
        },
        body: JSON.stringify({ [field]: value }),
      });

      if (!res.ok) throw new Error("Update failed");
      
      const updated = await res.json();
      if (updated.token) login(updated.token);
      
      setUserData(prev => ({ ...prev, ...updated }));
      setActiveForm(null);
      setFormData({ username: "", currentPassword: "", newPassword: "", picture: "" });
      showSuccess("Profile updated successfully!");
    } catch (err) {
      showError("Update failed");
    }
  };

  const handleChangePassword = async () => {
    try {
      const res = await fetch(`${apiUrl}/users/${userData.id}/change-password`, {
        method: "POST",
        headers: { 
          "Content-Type": "application/json",
          "Authorization": `Bearer ${localStorage.getItem("token")}`
        },
        body: JSON.stringify({
          currentPassword: formData.currentPassword,
          newPassword: formData.newPassword,
        }),
      });
      
      if (!res.ok) throw new Error("Password change failed");
      
      setActiveForm(null);
      setFormData({ username: "", currentPassword: "", newPassword: "", picture: "" });
      showSuccess("Password changed successfully!");
    } catch (err) {
      showError("Password change failed");
    }
  };

  const handlePictureUpload = async (e) => {
    e.preventDefault();
    if (!formData.pictureFile) {
      showError("Please select a file to upload.");
      return;
    }
    try {
      const uploadData = new FormData();
      uploadData.append("file", formData.pictureFile);
      const res = await fetch(`${apiUrl}/users/${user.id}/upload-profile-picture`, {
        method: "POST",
        headers: {
          "Authorization": `Bearer ${localStorage.getItem("token")}`
        },
        body: uploadData
      });
      if (!res.ok) throw new Error("Upload failed");
      const data = await res.json();
      setUserData(prev => ({ ...prev, profilePicture: data.profilePictureUrl }));
      setActiveForm(null);
      setFormData({ username: "", currentPassword: "", newPassword: "", pictureFile: null });
      showSuccess("Profile picture updated!");
    } catch (err) {
      showError("Upload failed");
    }
  };

  const handleDeleteAccount = async () => {
    if (!window.confirm("Are you sure you want to delete your account? This action cannot be undone.")) return;
    
    try {
      const res = await fetch(`${apiUrl}/users/${user.id}`, {
        method: "DELETE",
        headers: {
          "Authorization": `Bearer ${localStorage.getItem("token")}`
        }
      });
      
      if (res.status === 204) {
        showSuccess("Account deleted. Logging out...");
        logout();
        window.location.href = "/login";
      } else if (res.status === 403) {
        showError("You are not authorized to delete this account.");
      } else {
        const err = await res.text();
        showError("Delete failed: " + err);
      }
    } catch (err) {
      showError("Delete failed: " + err.message);
    }
  };

  if (!user?.userName) {
    return (
      <div className="container">
        <div className="card" style={{ textAlign: "center", padding: "3rem" }}>
          <h2>Please log in to view your profile</h2>
          <button className="btn btn-primary" onClick={() => window.location.href = "/login"}>
            Sign In
          </button>
        </div>
      </div>
    );
  }

  if (loading) return (
    <div className="container">
      <LoadingSkeleton type="profile" />
    </div>
  );

  return (
    <div className="container fade-in">
      <div className="card" style={{ maxWidth: "600px", margin: "2rem auto" }}>
        <div className="profile-header" style={{ textAlign: "center", marginBottom: "2rem" }}>
          {/* Debugging to see filepath */}
          {userData.profilePicture} 
          <img
            src={userData.profilePicture ? userData.profilePicture : `${apiUrl}/profile-pictures/default.png`}
            alt="Profile"
            style={{
              width: "120px",
              height: "120px",
              borderRadius: "50%",
              objectFit: "cover",
              border: "4px solid var(--primary-color)",
              marginBottom: "1rem"
            }}
          />
          <h2 style={{ color: "var(--primary-color)", marginBottom: "0.5rem" }}>
            {userData?.userName}
          </h2>
          <p style={{ color: "var(--text-secondary)" }}>Member since {new Date().getFullYear()}</p>
        </div>

        <div className="profile-actions">
          <div className="action-buttons" style={{ display: "flex", gap: "1rem", marginBottom: "2rem", flexWrap: "wrap" }}>
            <button 
              className="btn btn-secondary"
              onClick={() => setActiveForm(activeForm === "username" ? null : "username")}
            >
              Change Username
            </button>
            <button 
              className="btn btn-secondary"
              onClick={() => setActiveForm(activeForm === "password" ? null : "password")}
            >
              Change Password
            </button>
            <button 
              className="btn btn-secondary"
              onClick={() => setActiveForm(activeForm === "picture" ? null : "picture")}
            >
              Change Picture
            </button>
          </div>

          {/* Username Form */}
          {activeForm === "username" && (
            <div className="form-container slide-in-left">
              <h3>Change Username</h3>
              <div className="form-group">
                <input
                  className="form-control"
                  value={formData.username}
                  onChange={e => setFormData(prev => ({ ...prev, username: e.target.value }))}
                  placeholder="New username"
                  required
                />
              </div>
              <div style={{ display: "flex", gap: "1rem" }}>
                <button 
                  className="btn btn-primary"
                  onClick={() => handleUpdate("userName", formData.username)}
                  disabled={!formData.username}
                >
                  Save
                </button>
                <button 
                  className="btn btn-secondary"
                  onClick={() => setActiveForm(null)}
                >
                  Cancel
                </button>
              </div>
            </div>
          )}

          {/* Password Form */}
          {activeForm === "password" && (
            <div className="form-container slide-in-left">
              <h3>Change Password</h3>
              <div className="form-group">
                <input
                  type="password"
                  className="form-control"
                  value={formData.currentPassword}
                  onChange={e => setFormData(prev => ({ ...prev, currentPassword: e.target.value }))}
                  placeholder="Current password"
                  required
                />
              </div>
              <div className="form-group">
                <input
                  type="password"
                  className="form-control"
                  value={formData.newPassword}
                  onChange={e => setFormData(prev => ({ ...prev, newPassword: e.target.value }))}
                  placeholder="New password"
                  required
                />
              </div>
              <div style={{ display: "flex", gap: "1rem" }}>
                <button 
                  className="btn btn-primary"
                  onClick={handleChangePassword}
                  disabled={!formData.currentPassword || !formData.newPassword}
                >
                  Save
                </button>
                <button 
                  className="btn btn-secondary"
                  onClick={() => setActiveForm(null)}
                >
                  Cancel
                </button>
              </div>
            </div>
          )}

          {/* Picture Form */}
          {activeForm === "picture" && (
            <div className="form-container slide-in-left">
              <h3>Change Picture</h3>
              <form onSubmit={handlePictureUpload}>
                <div className="form-group">
                  <input
                    type="file"
                    accept="image/*"
                    className="form-control"
                    onChange={e => setFormData(prev => ({ ...prev, pictureFile: e.target.files[0] }))}
                    required
                  />
                </div>
                <div style={{ display: "flex", gap: "1rem" }}>
                  <button 
                    className="btn btn-primary"
                    type="submit"
                    disabled={!formData.pictureFile}
                  >
                    Upload
                  </button>
                  <button 
                    className="btn btn-secondary"
                    type="button"
                    onClick={() => setActiveForm(null)}
                  >
                    Cancel
                  </button>
                </div>
              </form>
            </div>
          )}
        </div>

        <div className="danger-zone" style={{ 
          marginTop: "3rem", 
          padding: "1.5rem", 
          border: "2px solid #ff4444", 
          borderRadius: "var(--border-radius)",
          backgroundColor: "rgba(255, 68, 68, 0.05)"
        }}>
          <h3 style={{ color: "#ff4444", marginBottom: "1rem" }}>Danger Zone</h3>
          <p style={{ marginBottom: "1rem", color: "var(--text-secondary)" }}>
            Once you delete your account, there is no going back.
          </p>
          <button 
            className="btn"
            style={{ backgroundColor: "#ff4444", color: "white" }}
            onClick={handleDeleteAccount}
          >
            Delete Account
          </button>
        </div>
      </div>
    </div>
  );
}

export default Profile;