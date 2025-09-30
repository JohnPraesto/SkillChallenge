import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { LoadingSkeleton } from "./LoadingSkeleton";
import { SearchAndFilter } from "./SearchAndFilter";
import { useToast } from "./ToastContext";

function Challenges() {
  const [challenges, setChallenges] = useState([]);
  const [filteredChallenges, setFilteredChallenges] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const { showError } = useToast();
  const apiUrl = import.meta.env.VITE_API_URL;

    useEffect(() => {
    Promise.all([
      fetch(apiUrl + "/api/challenges").then(res => res.json()),
      fetch(apiUrl + "/api/categories").then(res => res.json())
    ])
    .then(([challengesData, categoriesData]) => {
      setChallenges(challengesData);
      setCategories(categoriesData);
      setLoading(false);
    })
    .catch(err => {
      showError("Failed to load challenges");
      setLoading(false);
    });
  }, [showError]);

  const now = new Date();
  const openChallenges = filteredChallenges.filter(ch => new Date(ch.endDate) > now);
  const closedChallenges = filteredChallenges.filter(ch => new Date(ch.endDate) <= now && ch.resultsSubmitted === false);
  const finishedChallenges = filteredChallenges.filter(ch => new Date(ch.votePeriodEnd) <= now && ch.resultsSubmitted === true);

  const ChallengeCard = ({ challenge, index, status }) => (
    <div 
      className="card challenge-card stagger-item" 
      onClick={() => navigate(`/challenges/${challenge.challengeId}`)}
      style={{ animationDelay: `${index * 0.1}s` }}
    >
      {challenge.subCategory?.imagePath && (
        <img
          src={
            challenge.subCategory?.imagePath?.startsWith("http")
              ? challenge.subCategory.imagePath
              : `${apiUrl}/${challenge.subCategory.imagePath}`
          }
          alt={challenge.subCategory.subCategoryName || "Category"}
          loading="lazy"
        />
      )}
      <div className="challenge-title">{challenge.challengeName}</div>
      <div className="challenge-meta">
        {status === "open" && (
          <>Join Before: {new Date(challenge.endDate).toLocaleDateString('en-US', {month: 'long', day: 'numeric'})}</>
        )}
        {status === "closed" && (
          <>Voting Closes: {new Date(challenge.votePeriodEnd).toLocaleDateString('en-US', {month: 'long', day: 'numeric'})}</>
        )}
        {status === "finished" && (
          <>Is Archived: {new Date(challenge.isTakenDown).toLocaleDateString('en-US', {month: 'long', day: 'numeric'})}</>
        )}
      </div>
      <div className="challenge-meta">
        By: {challenge.creatorUserName}
      </div>
    </div>
  );

  return (
    <div className="container fade-in">
      <SearchAndFilter
        challenges={challenges}
        onFilteredChallenges={setFilteredChallenges}
        categories={categories}
      />

      <div className="three-column-challenges">
      {/* <div className="challenges-grid"> */}
        {/* Open Challenges */}
        <div className="challenge-column">
          <h2 className="challenge-header" style={{ color: "var(--primary-color)" }}>
            🔥 Open ({openChallenges.length})
          </h2>
          {openChallenges.length === 0 ? (
            <div className="card bounce-in" style={{ textAlign: "center", padding: "2rem" }}>
              <p>No open challenges found.</p>
            </div>
          ) : (
            openChallenges.map((ch, index) => (
              <ChallengeCard key={ch.challengeId} challenge={ch} index={index} status="open"/>
            ))
          )}
        </div>

        {/* Closed Challenges */}
        <div className="challenge-column">
          <h2 className="challenge-header" style={{ color: "#666" }}>
            ✅ Closed ({closedChallenges.length})
          </h2>
          {closedChallenges.length === 0 ? (
            <div className="card bounce-in" style={{ textAlign: "center", padding: "2rem" }}>
              <p>No closed challenges found.</p>
            </div>
          ) : (
            closedChallenges.map((ch, index) => (
              <ChallengeCard key={ch.challengeId} challenge={ch} index={index} status="closed"/>
            ))
          )}
        </div>

        {/* Finished Challenges */}
        <div className="challenge-column">
          <h2 className="challenge-header" style={{ color: "#999" }}>
            🏁 Finished ({finishedChallenges.length})
          </h2>
          {finishedChallenges.length === 0 ? (
            <div className="card bounce-in" style={{ textAlign: "center", padding: "2rem" }}>
              <p>No finished challenges found.</p>
            </div>
          ) : (
            finishedChallenges.map((ch, index) => (
              <ChallengeCard key={ch.challengeId} challenge={ch} index={index} status="finished"/>
            ))
          )}
        </div>
      </div>
      <div style={{ textAlign: "center", margin: "3rem 0 1.5rem 0" }}>
        <button
          className="footer-archived-btn"
          onClick={() => navigate("/archived-challenges")}
          style={{
            background: "none",
            color: "var(--primary-color, #222)",
            border: "none",
            fontSize: "1.2rem",
            cursor: "pointer"
          }}
        >
          📦 Archived Challenges
        </button>
      </div>
    </div>
  );
}

export default Challenges;