import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

// In here the users past and active challenges
// is to be seen. And their rating.

function UserDetail() {
  const { userName } = useParams();
  const [user, setUser] = useState(null);
  const [error, setError] = useState("");
  const apiUrl = import.meta.env.VITE_API_URL;

useEffect(() => {
  fetch(`${apiUrl}/users/username/${userName}`)
    .then(res => {
      if (!res.ok) {
        return res.text().then(text => { throw new Error(text); });
      }
      return res.json();
    })
    .then(data => setUser(data))
    .catch(err => setError(err.message));
}, [userName]);

  if (error) return <div style={{ color: "red" }}>{error}</div>;
  if (!user) return <div>Loading...</div>;

  return (
    <div>
      <h2>User: {user.userName}</h2>
      <img 
      // src={user.profilePicture ? user.profilePicture : `${apiUrl}/profile-pictures/default.png`}
      src={user.profilePicture ? user.profilePicture.startsWith("http")
                  ? user.profilePicture
                  : `${apiUrl}${user.profilePicture}`
                  : `${apiUrl}/profile-pictures/default.png`
                  }
      style={{width: "200px", height: "200px", objectFit: "cover", border: "3px solid var(--primary-color)"}}/>
    </div>
  );
}

export default UserDetail;