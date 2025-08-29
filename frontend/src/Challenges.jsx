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
      fetch(apiUrl + "/challenges").then(res => res.json()),
      fetch(apiUrl + "/categories").then(res => res.json())
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

  console.log(closedChallenges)
  console.log(filteredChallenges)

  const ChallengeCard = ({ challenge, index }) => (
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
        Ends: {new Date(challenge.endDate).toLocaleDateString()}
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
              <ChallengeCard key={ch.challengeId} challenge={ch} index={index} />
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
              <ChallengeCard key={ch.challengeId} challenge={ch} index={index} />
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
              <ChallengeCard key={ch.challengeId} challenge={ch} index={index} />
            ))
          )}
        </div>
      </div>
    </div>
  );
}

export default Challenges;