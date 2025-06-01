import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

// In here the users past and active challenges
// is to be seen. And their rating.

function UserDetail() {
  const { userName } = useParams();
  const [user, setUser] = useState(null);
  const [error, setError] = useState("");

useEffect(() => {
  fetch(`https://localhost:7212/users/username/${userName}`)
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
      <h2>Profile Picture: {user.profilePicture}</h2>
    </div>
  );
}

export default UserDetail;