import React, { useEffect, useState } from "react";
import { useAuth } from "./AuthContext";

function Profile() {
  const { user } = useAuth();
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
    if (!user?.userName) return;
    fetch(`https://localhost:7212/users/${user?.userName}`)
      .then(res => res.ok ? res.json() : Promise.reject("Failed to fetch user"))
      .then(data => setUserData(data))
      .catch(err => setError(err));
  }, [user?.userName]);

  // Update username or picture
  const handleUpdateUser = async (field, value) => {
    setMessage("");
    try {
      const res = await fetch(`https://localhost:7212/users/${user.id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ [field]: value }),
      });
      if (!res.ok) {
        const err = await res.text();
        setMessage("Update failed: " + err);
        return;
      }
      const updated = await res.json();
      setUser({ ...user, ...updated });
      setMessage("Update successful!");
    } catch (err) {
      setMessage("Update failed: " + err.message);
    }
  };

  // Change password
  const handleChangePassword = async () => {
    setMessage("");
    try {
      const res = await fetch(`https://localhost:7212/users/${user.id}/change-password`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
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
      <div><strong>Username:</strong> {user.userName}</div>
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
      </div>
    </div>
  );
}

export default Profile;