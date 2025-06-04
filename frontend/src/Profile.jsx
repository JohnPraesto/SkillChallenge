import React, { useEffect, useState } from "react";
import { useAuth } from "./AuthContext";

// In here the users past and active challenges
// is to be seen. And their rating.

function Profile() {
  const { user, login, logout } = useAuth();
  const [userData, setUserData] = useState(null);
  const [error, setError] = useState("");
  const [showUsernameForm, setShowUsernameForm] = useState(false);
  const [showPasswordForm, setShowPasswordForm] = useState(false);
  const [showPictureForm, setShowPictureForm] = useState(false);
  const [newUsername, setNewUsername] = useState("");
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [newPicture, setNewPicture] = useState("");
  const [message, setMessage] = useState("");

  useEffect(() => {
  console.log("AuthContext user:", user);
}, [user]);

  // Fetch user info on mount
  useEffect(() => {
    if (!user?.id) return;
    fetch(`https://localhost:7212/users/id/${user.id}`) // <-- id in route is new, should not use userName anymore?
      .then(res => res.ok ? res.json() : Promise.reject("Failed to fetch user"))
      .then(data => setUserData(data))
      .catch(err => setError(err));
  }, [user?.id]);

  // Update username or picture
  const handleUpdateUser = async (field, value) => {
    setMessage("");
    try {
      const res = await fetch(`https://localhost:7212/users/${user.id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json",
          "Authorization": `Bearer ${localStorage.getItem("token")}`
         },
        body: JSON.stringify({ [field]: value }),
      });
      if (!res.ok) {
        const err = await res.text();
        setMessage("Update failed: " + err);
        return;
      }
      const updated = await res.json();
      if (updated.token) {
        login(updated.token); // Update AuthContext with new token
        setUserData(null);    // Force refetch with new username
      }
      setUserData({ ...userData, ...updated });
      setMessage("Update successful!");
    } catch (err) {
      setMessage("Update failed: " + err.message);
    }
  };

  // Change password
  const handleChangePassword = async () => {
    setMessage("");
    try {
      const res = await fetch(`https://localhost:7212/users/${userData.id}/change-password`, {
        method: "POST",
        headers: { "Content-Type": "application/json",
          "Authorization": `Bearer ${localStorage.getItem("token")}`
         },
        body: JSON.stringify({
          currentPassword,
          newPassword,
        }),
      });
      if (!res.ok) {
        const err = await res.text();
        setMessage("Password change failed: " + err);
        return;
      }
      setMessage("Password changed successfully!");
    } catch (err) {
      setMessage("Password change failed: " + err.message);
    }
  };

  const handleDeleteAccount = async () => {
  if (!window.confirm("Are you sure you want to delete your account? This action cannot be undone.")) return;
  setMessage("");
  try {
    const res = await fetch(`https://localhost:7212/users/${user.id}`, {
      method: "DELETE",
      headers: {
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });
    if (res.status === 204) {
      setMessage("Account deleted. Logging out...");
      logout();
      // Optionally, redirect to home or login page:
      window.location.href = "/login";
    } else if (res.status === 403) {
      setMessage("You are not authorized to delete this account.");
    } else {
      const err = await res.text();
      setMessage("Delete failed: " + err);
    }
  } catch (err) {
    setMessage("Delete failed: " + err.message);
  }
};

  console.log("Profile.jsx user:", user);
  if (!user?.userName) return <div>Please log in to view your profile.</div>;
  if (error) return <div style={{ color: "red" }}>{error}</div>;
  if (!userData) return <div>Loading...</div>;

  return (
    <div style={{ maxWidth: 400, margin: "2em auto", textAlign: "center" }}>
      <h2>My Profile</h2>
      <img
        src={user.profilePicture || "/default-profile.png"}
        alt="Profile"
        style={{ width: 100, height: 100, borderRadius: "50%", objectFit: "cover" }}
      />
      <div><strong>Username:</strong> {userData.userName}</div>
      {message && <div style={{ color: "green", margin: "1em 0" }}>{message}</div>}

      <div style={{ marginTop: "2em" }}>
        <button onClick={() => setShowUsernameForm(v => !v)}>Change Username</button>
        {showUsernameForm && (
          <form
            onSubmit={e => {
              e.preventDefault();
              handleUpdateUser("userName", newUsername);
            }}
            style={{ margin: "1em 0" }}
          >
            <input
              value={newUsername}
              onChange={e => setNewUsername(e.target.value)}
              placeholder="New username"
              required
            />
            <button type="submit">Save</button>
          </form>
        )}

        <button onClick={() => setShowPasswordForm(v => !v)} style={{ marginLeft: 8 }}>
          Change Password
        </button>
        {showPasswordForm && (
          <form
            onSubmit={e => {
              e.preventDefault();
              handleChangePassword();
            }}
            style={{ margin: "1em 0" }}
          >
            <input
              type="password"
              value={currentPassword}
              onChange={e => setCurrentPassword(e.target.value)}
              placeholder="Current password"
              required
            />
            <input
              type="password"
              value={newPassword}
              onChange={e => setNewPassword(e.target.value)}
              placeholder="New password"
              required
            />
            <button type="submit">Save</button>
          </form>
        )}

        <button onClick={() => setShowPictureForm(v => !v)} style={{ marginLeft: 8 }}>
          Change Picture
        </button>
        {showPictureForm && (
          <form
            onSubmit={e => {
              e.preventDefault();
              handleUpdateUser("profilePicture", newPicture);
            }}
            style={{ margin: "1em 0" }}
          >
            <input
              value={newPicture}
              onChange={e => setNewPicture(e.target.value)}
              placeholder="New picture URL"
              required
            />
            <button type="submit">Save</button>
          </form>
        )}

        <button onClick={handleDeleteAccount} style={{ marginTop: 24, background: "#c00", color: "#fff" }}>
          Delete Account
        </button>
      </div>
    </div>
  );
}

export default Profile;