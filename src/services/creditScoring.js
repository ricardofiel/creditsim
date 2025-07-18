
function calculateCreditScore(customerData) {
  const { age, annualIncome, debtToIncomeRatio, loanAmount, creditHistory } = customerData;
  
  // Validate input data
  validateCustomerData(customerData);
  
  // Start with base score
  let score = 600;
  
  // Age adjustments
  if (age < 25) {
    score -= 50; // Young age penalty
  } else if (age > 60) {
    score -= 30; // Senior age penalty
  }
  
  // Income adjustments - more realistic bonuses
  if (annualIncome > 200000) {
    score += 120; // Very high income bonus
  } else if (annualIncome > 100000) {
    score += 80; // High income bonus
  } else if (annualIncome > 50000) {
    score += 40; // Moderate income bonus
  }
  
  // Debt-to-income ratio adjustments
  if (debtToIncomeRatio > 0.4) {
    score -= 80; // High debt-to-income penalty
  }
  
  // Credit history adjustments
  if (creditHistory === 'bad') {
    score -= 150; // Bad credit history penalty
  }
  
  // Loan amount vs income adjustments
  const loanToIncomeRatio = loanAmount / annualIncome;
  if (loanToIncomeRatio > 0.5) {
    score -= 50; // High loan-to-income penalty
  } else if (loanToIncomeRatio < 0.1) {
    score += 30; // Very low loan-to-income bonus
  } else if (loanToIncomeRatio < 0.25) {
    score += 15; // Low loan-to-income bonus
  }
  
  // Ensure score doesn't go below 300 or above 850 (typical FICO range)
  score = Math.max(300, Math.min(850, score));
  
  // Determine risk category
  const riskCategory = determineRiskCategory(score);
  
  return {
    score: Math.round(score),
    riskCategory
  };
}

/**
 * Determine risk category based on credit score
 * @param {number} score - Credit score
 * @returns {string} Risk category
 */
function determineRiskCategory(score) {
  if (score >= 750) {
    return 'Low risk';
  } else if (score >= 650) {
    return 'Medium risk';
  } else {
    return 'High risk';
  }
}

/**
 * Validate customer data inputs
 * @param {Object} customerData - Customer data to validate
 * @throws {Error} If validation fails
 */
function validateCustomerData(customerData) {
  const { age, annualIncome, debtToIncomeRatio, loanAmount, creditHistory } = customerData;
  
  const errors = [];
  
  // Age validation
  if (!Number.isInteger(age) || age < 18 || age > 120) {
    errors.push('Age must be an integer between 18 and 120');
  }
  
  // Annual income validation
  if (!Number.isFinite(annualIncome) || annualIncome < 0) {
    errors.push('Annual income must be a positive number');
  }
  
  // Debt-to-income ratio validation
  if (!Number.isFinite(debtToIncomeRatio) || debtToIncomeRatio < 0 || debtToIncomeRatio > 1) {
    errors.push('Debt-to-income ratio must be between 0 and 1');
  }
  
  // Loan amount validation
  if (!Number.isFinite(loanAmount) || loanAmount <= 0) {
    errors.push('Loan amount must be a positive number');
  }
  
  // Credit history validation
  if (!['good', 'bad'].includes(creditHistory)) {
    errors.push('Credit history must be either "good" or "bad"');
  }
  
  if (errors.length > 0) {
    throw new Error(`Validation failed: ${errors.join(', ')}`);
  }
}

/**
 * Get scoring criteria explanation
 * @returns {Object} Explanation of scoring criteria
 */
function getScoringCriteria() {
  return {
    baseScore: 600,
    adjustments: {
      age: {
        under25: -50,
        over60: -30
      },
      income: {
        over50k: +40
      },
      debtToIncomeRatio: {
        over40percent: -80
      },
      creditHistory: {
        bad: -150
      },
      loanToIncomeRatio: {
        over50percent: -50
      }
    },
    riskCategories: {
      lowRisk: '750+',
      mediumRisk: '650-749',
      highRisk: 'Below 650'
    }
  };
}

module.exports = {
  calculateCreditScore,
  determineRiskCategory,
  validateCustomerData,
  getScoringCriteria
};
