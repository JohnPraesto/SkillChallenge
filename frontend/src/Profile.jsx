import React, { useEffect, useState } from "react";
import { useAuth } from "./AuthContext";
import { useToast } from "./ToastContext";
import { ThemeToggle } from "./ThemeToggle";
import { LoadingSkeleton } from "./LoadingSkeleton";

function Profile() {
  const { user, login, logout } = useAuth();
  const [archivedChallenges, setArchivedChallenges] = useState([]);
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
  const [notifSettings, setNotifSettings] = useState({
    notifyTwoDaysBeforeEndDate: true,
    notifyOnEndDate: true,
    notifyOnVotingEnd: true,
  });
  const [notifLoading, setNotifLoading] = useState(false);
  const apiUrl = import.meta.env.VITE_API_URL;

  const handleNotifChange = (e) => {
    const { name, checked } = e.target;
    setNotifSettings((prev) => ({ ...prev, [name]: checked }));
  };

  const saveNotifSettings = async () => {
    setNotifLoading(true);
    try {
      const res = await fetch(`${apiUrl}/api/users/${user.id}/notification-settings`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          "Authorization": `Bearer ${localStorage.getItem("token")}`,
        },
        body: JSON.stringify({
          notifyTwoDaysBeforeEndDate: notifSettings.notifyTwoDaysBeforeEndDate,
          notifyOnEndDate: notifSettings.notifyOnEndDate,
          notifyOnVotingEnd: notifSettings.notifyOnVotingEnd,
        }),
      });
      if (!res.ok) throw new Error("Failed to update notification settings");
      showSuccess("Notification settings updated!");
    } catch (err) {
      showError("Failed to update notification settings");
    }
    setNotifLoading(false);
  };

  useEffect(() => {
  }, [user]);

  useEffect(() => {
    if (!user?.id) return;
    
    fetch(`${apiUrl}/api/users/id/${user.id}`)
      .then(res => res.ok ? res.json() : Promise.reject("Failed to fetch user"))
      .then(data => {
        setUserData(data);
        setLoading(false);
        setNotifSettings({
          notifyTwoDaysBeforeEndDate: data.notifyTwoDaysBeforeEndDate ?? true,
          notifyOnEndDate: data.notifyOnEndDate ?? true,
          notifyOnVotingEnd: data.notifyOnVotingEnd ?? true,
        });
      })
      .catch(err => {
        showError("Failed to load profile");
        setLoading(false);
      });

      // Fetch archived challenges for this user
    fetch(`${apiUrl}/api/archived-challenges/${user.id}`)
      .then(res => res.ok ? res.json() : Promise.reject("Failed to fetch archived challenges"))
      .then(data => setArchivedChallenges(data))
      .catch(() => setArchivedChallenges([]));
  }, [user?.id, showError]);

  const handleUpdate = async (field, value) => {
    try {
      const res = await fetch(`${apiUrl}/api/users/${user.id}`, {
        method: "PUT",
        headers: { 
          "Content-Type": "application/json",
          "Authorization": `Bearer ${localStorage.getItem("token")}`
        },
        body: JSON.stringify({ [field]: value }),
      });

      if (!res.ok) {
        let errorMsg = "Update failed";
        try {
          const errorData = await res.json();
          if (Array.isArray(errorData)) {
            errorMsg = errorData.map(e => e.description || e.code || JSON.stringify(e)).join(" ");
          } else if (typeof errorData === "string") {
            errorMsg = errorData;
          } else if (errorData?.message) {
            errorMsg = errorData.message;
          }
        } catch {
          // fallback to default error message
        }
        showError(errorMsg);
        return;
      }
      
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
      const res = await fetch(`${apiUrl}/api/users/${user.id}/change-password`, {
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
      const res = await fetch(`${apiUrl}/api/users/${user.id}/upload-profile-picture`, {
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
      const res = await fetch(`${apiUrl}/api/users/${user.id}`, {
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
    <div className="profile-card-container">
      <div className="profile-card">
        <div className="profile-header">
          <img
            src={userData.profilePicture ? userData.profilePicture.startsWith("http")
                  ? userData.profilePicture
                  : `${apiUrl}/${userData.profilePicture}`
                  : `${apiUrl}/profile-pictures/default.png`
            }
            alt="Profile"
            style={{width: "120px", height: "120px", borderRadius: "50%", objectFit: "cover", order: "4px solid var(--primary-color)", marginBottom: "1rem"}}
          />
          <h2 style={{ color: "var(--primary-color)", marginBottom: "0.5rem" }}>
            {userData?.userName}
          </h2>
        </div>

        <div className="profile-actions">
          <div className="profile-buttons">
            <div className="nav-link">
              <ThemeToggle />
            </div>
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

            {user.roles === "Admin" && (
            <button className="btn btn-warning" onClick={() => window.location.href = "/admin"}>
              Admin Dashboard
            </button>)}

            <button 
              className="btn"
              style={{ backgroundColor: "#ff4444", color: "white"}}
              onClick={handleDeleteAccount}
            >
            Delete Account
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

                {/* Notification Settings */}
        <div className="notification-settings">
          <h3 style={{ textAlign: "center", marginBottom: "1.5rem" }}>Email Notification Settings</h3>
          <div className="notification-checkbox-row">
            <label className="notification-label">
              <input
                type="checkbox"
                name="notifyTwoDaysBeforeEndDate"
                checked={notifSettings.notifyTwoDaysBeforeEndDate}
                onChange={handleNotifChange}
                style={{ transform: "scale(1.2)" }}
              />
              Notify 2 days before challenges ends
            </label>
            <label className="notification-label">
              <input
                type="checkbox"
                name="notifyOnEndDate"
                checked={notifSettings.notifyOnEndDate}
                onChange={handleNotifChange}
                style={{ transform: "scale(1.2)" }}
              />
              Notify when voting begins
            </label>
            <label className="notification-label">
              <input
                type="checkbox"
                name="notifyOnVotingEnd"
                checked={notifSettings.notifyOnVotingEnd}
                onChange={handleNotifChange}
                style={{ transform: "scale(1.2)" }}
              />
              Notify when voting has finished
            </label>
          </div>  
          <button
            className="btn btn-primary"
            style={{width: "200px", marginBottom: "0.5rem"}}
            onClick={saveNotifSettings}
            disabled={notifLoading}
          >
            {notifLoading ? "Saving..." : "Save Notification Settings"}
          </button>
        </div>

        <div className="challenge-history">
          <h3 style={{ textAlign: "center" }}>Your Challenge History</h3>
          {archivedChallenges.length === 0 ? (
            <div>No archived challenges found.</div>
          ) : (
            <div style={{ overflowX: "auto" }}>
              <table style={{ width: "100%", borderCollapse: "collapse", border: "1px solid #ccc" }}>
                <thead>
                  <tr>
                    <th style={{ textAlign: "left", padding: "8px", border: "1px solid #ccc"}}>Challenge Name</th>
                    <th style={{ textAlign: "left", padding: "8px", border: "1px solid #ccc"}}>Subcategory Name</th>
                    <th style={{ textAlign: "left", padding: "8px", border: "1px solid #ccc"}}>End Date</th>
                    <th style={{ textAlign: "left", padding: "8px", border: "1px solid #ccc"}}>Placement</th>
                    <th style={{ textAlign: "left", padding: "8px", border: "1px solid #ccc"}}>Rating Change</th>
                  </tr>
                </thead>
                <tbody>
                  {archivedChallenges.map((challenge, idx) => {
                    // Find the user's entry in the Users array
                    const userEntry = Array.isArray(challenge.users || challenge.Users)
                      ? (challenge.users || challenge.Users).find(
                          u => u.userId === user.id || u.UserId === user.id
                        )
                      : null;
                    return (
                      <tr key={challenge.archivedChallengeId || challenge.id || idx}>
                        <td style={{ padding: "8px", border: "1px solid #ccc" }}>
                          {challenge.challengeName || challenge.ChallengeName}
                        </td>
                        <td style={{ padding: "8px", border: "1px solid #ccc" }}>
                          {challenge.subCategoryName || challenge.SubCategoryName}
                        </td>
                        <td style={{ padding: "8px", border: "1px solid #ccc" }}>
                          {new Date(challenge.endDate || challenge.EndDate).toLocaleDateString('en-US', {year: 'numeric', month: 'long', day: 'numeric'})}
                        </td>
                        <td style={{ padding: "8px", border: "1px solid #ccc" }}>
                          {userEntry
                            ? `${userEntry.placement ?? userEntry.Placement} / ${(challenge.users || challenge.Users)?.length ?? 1}`
                            : "-"}
                        </td>
                        <td style={{ padding: "8px", border: "1px solid #ccc" }}>
                          {userEntry ? (
                            <span style={{ color: (userEntry.ratingChange ?? userEntry.RatingChange) >= 0 ? "green" : "red" }}>
                              {(userEntry.ratingChange ?? userEntry.RatingChange) >= 0 ? "+" : ""}
                              {userEntry.ratingChange ?? userEntry.RatingChange}
                            </span>
                          ) : "-"}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default Profile;