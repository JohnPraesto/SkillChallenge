import React from "react";

function About() {
  return (
    <div className="about-container">
      <div className="about-card">
        <h1 className="about-title">Welcome to Skill Challenge!</h1>
        <ul className="about-list">
          <li>Challenge friends and strangers in anything!</li>
          <li>You can create your own challenges or browse to join challenges already created by community members</li>
          <li>Participants of a challenge can upload their results before the challenge end date</li>
          <li>After the end date, community members can view and vote on the results!</li>
          <li>The winners gain rating, the losers lose rating!</li>
          <li>Have fun!</li>
        </ul>
        <div className="about-contact">
          <strong>Contact:</strong> john_praesto@hotmail.com
        </div>
      </div>
    </div>
  );
}

export default About;