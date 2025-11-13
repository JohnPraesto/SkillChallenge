import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useAuth } from "./AuthContext";
import OpenChallengeDetails from "./OpenChallengeDetails";
import ClosedChallengeDetails from "./ClosedChallengeDetails";
import FinishedChallengeDetails from "./FinishedChallengeDetails";

function ChallengeDetails() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [challenge, setChallenge] = useState(null);
  const [error, setError] = useState("");
  const { user } = useAuth();
  const apiUrl = import.meta.env.VITE_API_URL;

  // Anonymous voting
  const fetchChallenge = () => {
    fetch(`${apiUrl}/api/challenges/${id}`, {
      method: "GET",
      credentials: "include"
    })
      .then(res => res.ok ? res.json() : Promise.reject("Failed to fetch challenge"))
      .then(data => {
        // Debug (temporary)
        console.log("Fetched challenge votedResultIdForCurrentClient:", data.votedResultIdForCurrentClient);
        setChallenge(data);
      })
      .catch(err => setError(err.toString()));
  };

  // // Authenticated voting
  // const fetchChallenge = () => {
  //   fetch(`${apiUrl}/api/challenges/${id}`)
  //     .then(res => res.ok ? res.json() : Promise.reject("Failed to fetch challenge"))
  //     .then(data => setChallenge(data))
  //     .catch(err => setError(err.toString()));
  // };

  useEffect(() => 
    {
      fetchChallenge();
    }, [id]);

  if (error) return <div style={{ color: "red" }}>{error}</div>;
  if (!challenge) return <div>Loading...</div>;

const now = new Date();
  const isOpen = new Date(challenge.endDate) > now;
  const isClosed = new Date(challenge.endDate) <= now && new Date(challenge.votePeriodEnd) > now;
  const isFinished = new Date(challenge.votePeriodEnd) <= now;
  const numberOfParticipants = challenge.joinedUsers ? challenge.joinedUsers.length : 0;

  const endDateStr = new Date(challenge.endDate).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });
  const voteEndStr = new Date(challenge.votePeriodEnd).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });

  const statusLabel = isOpen ? "Open for submissions" : isClosed ? "Voting in progress" : "Finished";
  const statusClass = isOpen ? "status-open" : isClosed ? "status-closed" : "status-finished";
  const participantsMax = challenge.numberOfParticipants || 0;
  const participantsPct = participantsMax > 0 ? Math.min(100, Math.round((numberOfParticipants / participantsMax) * 100)) : 0;

  return (
    <div className="challenge-details-base">
      <h1 style={{textAlign: "center", color: "var(--primary-color)"}}>{challenge.challengeName}</h1>
      <img
        src={
          challenge.subCategory?.imagePath?.startsWith("http")
            ? challenge.subCategory.imagePath
            : `${apiUrl}/${challenge.subCategory.imagePath}`
        }
        alt={challenge.subCategory.subCategoryName || "Category"}
        style={{ width: "auto", height: 300, objectFit: "cover", borderRadius: 8, marginBottom: 12 }}
      />

      {/* Meta card */}
      <div className="challenge-meta-card">
        <div className={`status-badge ${statusClass}`}>{statusLabel}</div>

        <div className="description-box">
          <div className="meta-label">📝 Description</div>
          <div className="meta-description">{challenge.description}</div>
        </div>

        <div className="meta-grid">
          <div className="meta-item">
            <div className="meta-label">👥 Participants</div>
            <div className="meta-value">
              <span className="participants-count">
                {numberOfParticipants}{participantsMax ? `/${participantsMax}` : ""}
              </span>
              <div className="participants-bar" aria-label="Participants progress">
                <div className="participants-fill" style={{ width: `${participantsPct}%` }} />
              </div>
            </div>
          </div>

          <div className="meta-item">
            <div className="meta-label">🏷️ Category</div>
            <div className="meta-value">{challenge.subCategory?.categoryName}</div>
          </div>

          <div className="meta-item">
            <div className="meta-label">🎯 Subcategory</div>
            <div className="meta-value">{challenge.subCategory?.subCategoryName}</div>
          </div>

          <div className="meta-item">
            <div className="meta-label">{isClosed ? "🗓️ Voting closes" : isFinished ? "🗓️ Voting closed" : "🗓️ Submission closes"}</div>
            <div className="meta-value">{isClosed || isFinished ? voteEndStr : endDateStr}</div>
          </div>
          
        </div>
      </div>

      {isOpen && (
        <OpenChallengeDetails
          challenge={challenge}
          user={user}
          navigate={navigate}
          apiUrl={apiUrl}
          fetchChallenge={fetchChallenge}
        />
      )}
      {isClosed && (
        <ClosedChallengeDetails
          challenge={challenge}
          user={user}
          navigate={navigate}
          apiUrl={apiUrl}
          fetchChallenge={fetchChallenge}
        />
      )}
      {isFinished && (
        <FinishedChallengeDetails
          challenge={challenge}
          user={user}
          navigate={navigate}
          apiUrl={apiUrl}
          fetchChallenge={fetchChallenge}
        />
      )}

      <div><strong>Challenge Created by:</strong> {challenge.creatorUserName}</div>
    </div>
  );
}

export default ChallengeDetails;