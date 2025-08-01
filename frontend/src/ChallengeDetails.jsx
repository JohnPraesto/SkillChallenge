import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useAuth } from "./AuthContext";
import OpenChallengeDetails from "./OpenChallengeDetails";
import ClosedChallengeDetails from "./ClosedChallengeDetails";

function ChallengeDetails() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [challenge, setChallenge] = useState(null);
  const [error, setError] = useState("");
  const { user } = useAuth();
  const apiUrl = import.meta.env.VITE_API_URL;

  const fetchChallenge = () => {
    fetch(`${apiUrl}/challenges/${id}`)
      .then(res => res.ok ? res.json() : Promise.reject("Failed to fetch challenge"))
      .then(data => setChallenge(data))
      .catch(err => setError(err.toString()));
  };

  useEffect(() => 
    {
      fetchChallenge();
    }, [id]);

  if (error) return <div style={{ color: "red" }}>{error}</div>;
  if (!challenge) return <div>Loading...</div>;

  const isClosed = new Date(challenge.endDate) <= new Date();

  return (
    <div style={{ maxWidth: 500, margin: "2em auto", padding: 24, border: "1px solid #ccc", borderRadius: 8 }}>
      <h2>{challenge.challengeName}</h2>
      <img
        src={`${apiUrl}/${challenge.subCategory.imagePath}`}
        alt={challenge.subCategory.subCategoryName || "Category"}
        style={{ width: 300, height: 300, objectFit: "cover", borderRadius: 8, marginBottom: 12 }}
      />
      <div><strong>End Date:</strong> {new Date(challenge.endDate).toLocaleString()}</div>
      <div><strong>Description:</strong> {challenge.description}</div>
      <div><strong>Created by:</strong> {challenge.creatorUserName}</div>
      {isClosed ? (
        <ClosedChallengeDetails
          challenge={challenge}
          user={user}
          navigate={navigate}
          apiUrl={apiUrl}
          fetchChallenge={fetchChallenge}
        />
      ) : (
        <OpenChallengeDetails
          challenge={challenge}
          user={user}
          navigate={navigate}
          apiUrl={apiUrl}
          fetchChallenge={fetchChallenge}
        />
      )}
    </div>
  );
}

export default ChallengeDetails;