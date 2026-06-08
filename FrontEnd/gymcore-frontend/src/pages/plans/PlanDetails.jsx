import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import { getPlan } from "../../api/planApi";

function PlanDetails() {

  const { id } = useParams();

  const [plan, setPlan] = useState(null);

  useEffect(() => {

    loadPlan();

  }, []);

  const loadPlan = async () => {

    const response = await getPlan(id);

    setPlan(response.data);
  };

  if (!plan)
    return <h3>Loading...</h3>;

  return (
    <div className="container mt-5">

      <div className="card">

        <div className="card-body">

          <h2>{plan.name}</h2>

          <p>Price: {plan.price}</p>

          <p>Duration: {plan.durationInDays}</p>

          <p>{plan.description}</p>

        </div>

      </div>

    </div>
  );
}

export default PlanDetails;